package main

import (
	"encoding/base64"
	"encoding/json"
	"fmt"
	"net"
	"os"
	"os/signal"
	"syscall"
	"time"

	uuid "github.com/satori/go.uuid"
)

const maxBufferSize = 4024

type messageSend struct {
	buf  []byte
	addr net.Addr
}

type client struct {
	id       string
	secret   string
	addr     net.Addr
	lastPing time.Time
}

type Server struct {
	addr        string
	sendChan    chan messageSend
	messageChan chan Message
	eventChan   chan *EventMessage
	pc          net.PacketConn
	parser      *Parser

	clientsSliceOwner *clientsOwner

	handlers     map[string]func(id string, buf string) error
	onDisconnect func(id string)
	onConnect    func(id string)
}

func NewServer(addr string) *Server {
	return &Server{
		addr:              addr,
		sendChan:          make(chan messageSend, bufferSize),
		messageChan:       make(chan Message, bufferSize),
		eventChan:         make(chan *EventMessage, bufferSize),
		parser:            NewParser(),
		clientsSliceOwner: newClientsOwner(),
		handlers:          make(map[string]func(string, string) error),
	}
}

func (s *Server) Send(buf []byte, addr net.Addr) {
	s.sendChan <- messageSend{buf, addr}
}

func (s *Server) On(event string, handler func(string, string) error) {
	s.handlers[event] = handler
}

func (s *Server) OnDisconnect(handler func(id string)) {
	s.onDisconnect = handler
}

func (s *Server) EmitTo(id string, event string, buf string) {
	cli, ok := <-s.clientsSliceOwner.GetClientByID(id)
	if !ok {
		return
	}
	buf2, err := json.Marshal(&EventMessage{Event: event, Payload: buf})
	if err != nil {
		return
	}
	s.Send(buf2, cli.addr)
}

func (s *Server) Broadcast(event string, buf string) {
	msg := &EventMessage{Event: event, Payload: buf}
	buf2, err := json.Marshal(msg)
	if err != nil {
		panic(err)
	}
	for cli := range s.clientsSliceOwner.RangeClient() {
		buf3 := make([]byte, len(buf2))
		copy(buf3, buf2)
		s.Send(buf3, cli.addr)
	}
}

const garbageDuration = 100 * time.Millisecond

func (s *Server) garbageClientCollect(done chan bool) {
	ticker := time.NewTicker(garbageDuration)
	for {
		select {
		case <-ticker.C:
			for cli := range s.clientsSliceOwner.RangeClient() {
				if time.Now().Sub(cli.lastPing) > time.Second {
					s.deleteClient(cli)
				}
			}
		case <-done:
			return
		}
	}
}

func (s *Server) Run() {
	pc, err := net.ListenPacket("udp", s.addr)
	if err != nil {
		panic(err)
	}
	s.pc = pc
	defer s.pc.Close()
	quit := make(chan bool)
	defer func() { close(quit) }()
	for i := 0; i < 4; i++ {
		go s.listenConn(quit)
	}
	for i := 0; i < 4; i++ {
		go s.listenMessageChan(quit)
	}
	for i := 0; i < 4; i++ {
		go s.listenSendChan(quit)
	}

	go s.clientsSliceOwner.Run(quit)
	go s.garbageClientCollect(quit)
	c := make(chan os.Signal, 2)
	signal.Notify(c, os.Interrupt, syscall.SIGTERM)
	<-c
}

func (s *Server) listenConn(done <-chan bool) {
	for {
		select {
		case <-done:
			return
		default:
			buf := make([]byte, maxBufferSize)
			n, addr, err := s.pc.ReadFrom(buf)
			if err != nil {
				fmt.Println(err)
				continue
			}
			msg, err := s.parser.ParseMessage(addr, buf[:n])
			if err != nil {
				fmt.Println(err)
				continue
			}
			s.messageChan <- msg
		}
	}
}

func (s *Server) listenSendChan(done <-chan bool) {
	for {
		select {
		case <-done:
			return
		case msgSend := <-s.sendChan:
			_, err := s.pc.WriteTo(msgSend.buf, msgSend.addr)
			if err != nil {
				fmt.Println(err)
				continue
			}
		}
	}
}

func (s *Server) listenMessageChan(done <-chan bool) {
	for {
		select {
		case <-done:
			return
		case msg := <-s.messageChan:
			s.handleMessage(msg)
		}
	}
}

func (s *Server) handleMessage(msg Message) {
	tmp := msg.GetAddr().String()
	if cli, ok := <-s.clientsSliceOwner.GetClientByAddr(tmp); ok {
		s.clientsSliceOwner.UpdatePing(cli.id)
	}
	switch msg := msg.(type) {
	case *DisconnectMessage:
		if cli, ok := <-s.clientsSliceOwner.GetClientByID(msg.ID); ok {
			if cli.addr.String() == msg.Addr.String() {
				s.deleteClient(cli)
			}
		}
	case *EventMessage:
		if handler, ok := s.handlers[msg.Event]; ok {
			err := handler(msg.ID, msg.Payload)
			if err != nil {
				fmt.Println(err)
			}
		}
	case *HandshakeMessage:
		if cli, ok := <-s.clientsSliceOwner.GetClientByAddr(msg.Addr.String()); ok {
			s.deleteClient(cli)
		}
		secret, id := s.createUser()
		cli := client{
			id:       id,
			secret:   secret,
			addr:     msg.Addr,
			lastPing: time.Now(),
		}
		s.addClient(cli)
		s.EmitTo(cli.id, "connected", `{"secret":"`+base64.StdEncoding.EncodeToString([]byte(secret))+`","id":"`+id+`"}`)
		if s.onConnect != nil {
			s.onConnect(cli.id)
		}
	}

}

func (s *Server) addClient(cli client) {
	<-s.clientsSliceOwner.AddClient(cli)
	s.parser.RegisterUser(cli.secret, cli.id)
}

func (s *Server) deleteClient(cli client) {
	<-s.clientsSliceOwner.RemoveClient(cli)
	s.parser.DeleteUser(cli.secret)
	if s.onDisconnect != nil {
		s.onDisconnect(cli.id)
	}
	s.Broadcast("disconnect", `{"id":"`+cli.id+`"}`)
}

func (s *Server) createUser() (string, string) {
	secret := string(uuid.NewV4().Bytes()[:16])
	id := uuid.NewV4().String()[:6]
	return secret, id
}

func (s *Server) OnConnect(f func(string)) {
	s.onConnect = f
}

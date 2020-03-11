package main

import (
	"fmt"
	"net"
	"os"
	"os/signal"
	"sync/atomic"
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
	id       int
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
	st          *Stats

	lastId int32

	clientsSliceOwner *clientsOwner

	handlers     map[byte]func(id int, buf []byte) error
	onDisconnect func(id int)
	onConnect    func(id int)
}

func NewServer(addr string) *Server {
	return &Server{
		addr:              addr,
		sendChan:          make(chan messageSend, bufferSize),
		messageChan:       make(chan Message, bufferSize),
		eventChan:         make(chan *EventMessage, bufferSize),
		parser:            NewParser(),
		clientsSliceOwner: newClientsOwner(),
		st:                NewStats(),
		lastId:            0,
		handlers:          make(map[byte]func(int, []byte) error),
	}
}

func (s *Server) Send(buf []byte, addr net.Addr) {
	s.sendChan <- messageSend{buf, addr}
}

func (s *Server) On(event byte, handler func(int, []byte) error) {
	s.handlers[event] = handler
}

func (s *Server) OnDisconnect(handler func(id int)) {
	s.onDisconnect = handler
}

func (s *Server) EmitTo(id int, event byte, buf []byte) {
	cli, ok := <-s.clientsSliceOwner.GetClientByID(id)
	if !ok {
		return
	}
	s.emitTo(&EventMessage{Event: event, Payload: buf}, cli.addr)
}

func (s *Server) emitTo(msg Message, addr net.Addr) {
	s.Send(msg.Marshal(), addr)
}

func (s *Server) Broadcast(event byte, buf []byte) {
	s.broadcast(&EventMessage{Event: event, Payload: buf})
}

func (s *Server) broadcast(msg Message) {
	buf := msg.Marshal()
	for cli := range s.clientsSliceOwner.RangeClient() {
		s.Send(buf, cli.addr)
	}
}

const garbageDuration = 100 * time.Millisecond
const statsTick = 10 * time.Millisecond

func (s *Server) garbageClientCollect(done chan bool) {
	ticker := time.NewTicker(garbageDuration)
	ticker2 := time.NewTicker(statsTick)
	for {
		select {
		case <-ticker.C:
			for cli := range s.clientsSliceOwner.RangeClient() {
				if time.Now().Sub(cli.lastPing) > 2*time.Second {
					go func() {
						s.deleteClient(cli)
						s.st.nGarbage.Value++
					}()
				}
			}
		case <-ticker2.C:
			s.st.Monitor(s)
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
	for i := 0; i < 2; i++ {
		go s.listenConn(quit)
	}
	for i := 0; i < 2; i++ {
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
				// fmt.Println(err)
				continue
			}
			s.st.nRecvBytes.count(n)
			s.st.nRecvPackets.count(1)
			s.st.nMsg.count(1)
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
			s.st.nSentBytes.count(len(msgSend.buf))
			s.st.nSentPackets.count(1)
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

func (s *Server) log(cli client, str string) {
	fmt.Println("[" + string(cli.id) + " " + cli.addr.String() + "] " + str)
}

func (s *Server) handleMessage(msg Message) {
	s.st.nEvent.count(1)
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
		if cli, ok := <-s.clientsSliceOwner.GetClientByID(msg.ID); ok {
			if handler, ok := s.handlers[msg.Event]; ok {
				err := handler(msg.ID, msg.Payload)
				if err != nil {
					s.log(cli, err.Error())
				}
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
		s.log(cli, "connected")
		s.emitTo(&HandshakeMessage{Secret: secret, ID: id}, cli.addr)
		if s.onConnect != nil {
			s.onConnect(cli.id)
		}
	}
	s.st.nDone.count(1)
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
	s.log(cli, "disconnected")
	s.broadcast(&DisconnectMessage{ID: cli.id})
}

func (s *Server) createUser() (string, int) {
	secret := string(uuid.NewV4().Bytes()[:16])
	id := atomic.AddInt32(&s.lastId, 1)
	return secret, int(id)
}

func (s *Server) OnConnect(f func(int)) {
	s.onConnect = f
}

package main

import (
	"bytes"
	"encoding/binary"
	"errors"
	"fmt"
	"net"
	"sync"
)

type Parser struct {
	mu    sync.RWMutex
	idMap map[string]int
}

func NewParser() *Parser {
	return &Parser{
		idMap: make(map[string]int),
	}
}

type Message interface {
	GetAddr() net.Addr
	Marshal() []byte
	sealed()
}

type HandshakeMessage struct {
	Addr   net.Addr `json:"-"`
	Secret string   `json:"secret"`
	ID     int      `json:"id"`
}

func (h *HandshakeMessage) GetAddr() net.Addr {
	return h.Addr
}

func (h *HandshakeMessage) Marshal() []byte {
	buf := new(bytes.Buffer)
	binary.Write(buf, binary.BigEndian, byte(0x01))
	binary.Write(buf, binary.BigEndian, int32(h.ID))
	if len(buf.Bytes()) < 5 {
		binary.Write(buf, binary.BigEndian, 0x00)
	}
	binary.Write(buf, binary.BigEndian, []byte(h.Secret))
	fmt.Println(buf.Bytes())
	return buf.Bytes()
}

func (h *HandshakeMessage) sealed() {}

type EventMessage struct {
	Addr    net.Addr `json:"-"`
	Secret  string   `json:"-"`
	ID      int      `json:"-"`
	Event   byte     `json:"event"`
	Payload []byte   `json:"payload"`
}

func (h *EventMessage) sealed() {}

func (h *EventMessage) GetAddr() net.Addr {
	return h.Addr
}

func (h *EventMessage) Marshal() []byte {
	buf := new(bytes.Buffer)
	binary.Write(buf, binary.BigEndian, byte(0x02))
	binary.Write(buf, binary.BigEndian, h.Event)
	binary.Write(buf, binary.BigEndian, h.Payload)
	return buf.Bytes()
}

type DisconnectMessage struct {
	Addr net.Addr `json:"-"`
	ID   int      `json:"id"`
}

func (h *DisconnectMessage) sealed() {}

func (h *DisconnectMessage) GetAddr() net.Addr {
	return h.Addr
}

func (h *DisconnectMessage) Marshal() []byte {
	buf := new(bytes.Buffer)
	binary.Write(buf, binary.BigEndian, byte(0x03))
	binary.Write(buf, binary.BigEndian, int32(h.ID))
	return buf.Bytes()
}

func (s *Parser) ParseMessage(addr net.Addr, buf []byte) (Message, error) {
	// 0: message type (1 byte)
	if len(buf) == 0 {
		return nil, errors.New("empty buffer")
	}
	messageType := buf[0]
	buf = buf[1:]
	switch messageType {
	case 0x01:
		return &HandshakeMessage{Addr: addr}, nil
	case 0x02:
		if len(buf) < 17 {
			return nil, errors.New("invalid size")
		}
		secret := string(buf[:16])
		buf = buf[16:]

		s.mu.RLock()
		id, ok := s.idMap[secret]
		s.mu.RUnlock()

		if !ok {
			return nil, errors.New("invalid secret")
		}

		event := buf[0]
		buf = buf[1:]

		return &EventMessage{Addr: addr, Secret: secret, ID: id, Event: event, Payload: buf}, nil
	case 0x03:
		if len(buf) < 16 {
			return nil, errors.New("invalid size")
		}
		secret := string(buf[:16])
		buf = buf[16:]

		s.mu.RLock()
		id, ok := s.idMap[secret]
		s.mu.RUnlock()
		if !ok {
			return nil, errors.New("no such user")
		}

		return &DisconnectMessage{Addr: addr, ID: id}, nil
	}
	return nil, errors.New("no such message type")
}

func (s *Parser) RegisterUser(secret string, id int) {
	s.mu.Lock()
	s.idMap[secret] = id
	s.mu.Unlock()
}

func (s *Parser) DeleteUser(secret string) {
	s.mu.Lock()
	delete(s.idMap, secret)
	s.mu.Unlock()
}

package main

import (
	"errors"
	"net"
	"sync"
)

type Parser struct {
	mu    sync.RWMutex
	idMap map[string]string
}

func NewParser() *Parser {
	return &Parser{
		idMap: make(map[string]string),
	}
}

type Message interface {
	GetAddr() net.Addr
	sealed()
}

type HandshakeMessage struct {
	Addr   net.Addr
	Secret string
	ID     string
}

func (h *HandshakeMessage) GetAddr() net.Addr {
	return h.Addr
}

func (h *HandshakeMessage) sealed() {}

type EventMessage struct {
	Addr    net.Addr `json:"-"`
	Secret  string   `json:"-"`
	ID      string   `json:"-"`
	Event   string   `json:"event"`
	Payload string   `json:"payload"`
}

func (h *EventMessage) sealed() {}

func (h *EventMessage) GetAddr() net.Addr {
	return h.Addr
}

type DisconnectMessage struct {
	Addr net.Addr
	ID   string
}

func (h *DisconnectMessage) sealed() {}

func (h *DisconnectMessage) GetAddr() net.Addr {
	return h.Addr
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
		if len(buf) < 16 {
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

		cutI := 0
		for i, d := range buf {
			if d == ':' {
				cutI = i
				break
			}
			if i == len(buf)-1 {
				return nil, errors.New("no event name")
			}
		}

		eventName := string(buf[:cutI])
		buf = buf[cutI+1:]

		return &EventMessage{Addr: addr, Secret: secret, ID: id, Event: eventName, Payload: string(buf)}, nil
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

func (s *Parser) RegisterUser(secret string, id string) {
	s.mu.Lock()
	s.idMap[secret] = id
	s.mu.Unlock()
}

func (s *Parser) DeleteUser(secret string) {
	s.mu.Lock()
	delete(s.idMap, secret)
	s.mu.Unlock()
}

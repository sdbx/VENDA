package main

import (
	"time"
)

type addClientOp struct {
	cli  client
	resp chan bool
}

type removeClientOp struct {
	cli  client
	resp chan bool
}

type getClientOp struct {
	byAddr bool
	addr   string
	id     int
	resp   chan client
}

type rangeClientOp struct {
	resp chan client
}

type clientsOwner struct {
	addClientChan    chan addClientOp
	removeClientChan chan removeClientOp
	getClientChan    chan getClientOp
	rangeClientChan  chan rangeClientOp
	updatePingChan   chan int

	clients       map[int]client
	clientsByAddr map[string]client
}

const bufferSize = 1024

func newClientsOwner() *clientsOwner {
	return &clientsOwner{
		addClientChan:    make(chan addClientOp, bufferSize),
		removeClientChan: make(chan removeClientOp, bufferSize),
		getClientChan:    make(chan getClientOp, bufferSize),
		rangeClientChan:  make(chan rangeClientOp, bufferSize),
		updatePingChan:   make(chan int, bufferSize),
		clients:          make(map[int]client, bufferSize),
		clientsByAddr:    make(map[string]client, bufferSize),
	}
}

func (c *clientsOwner) AddClient(cli client) <-chan bool {
	out := make(chan bool, 1)
	c.addClientChan <- addClientOp{
		cli:  cli,
		resp: out,
	}
	return out
}

func (c *clientsOwner) RemoveClient(cli client) <-chan bool {
	out := make(chan bool, 1)
	c.removeClientChan <- removeClientOp{
		cli:  cli,
		resp: out,
	}
	return out
}

func (c *clientsOwner) GetClientByID(id int) <-chan client {
	out := make(chan client, 1)
	c.getClientChan <- getClientOp{
		byAddr: false,
		id:     id,
		resp:   out,
	}
	return out
}

func (c *clientsOwner) GetClientByAddr(addr string) <-chan client {
	out := make(chan client, 1)
	c.getClientChan <- getClientOp{
		byAddr: true,
		addr:   addr,
		resp:   out,
	}
	return out
}

func (c *clientsOwner) RangeClient() <-chan client {
	out := make(chan client, 1)
	c.rangeClientChan <- rangeClientOp{
		resp: out,
	}
	return out
}

func (c *clientsOwner) UpdatePing(id int) {
	c.updatePingChan <- id
}

func (c *clientsOwner) Run(done <-chan bool) {
	for {
		select {
		case op := <-c.addClientChan:
			c.clients[op.cli.id] = op.cli
			c.clientsByAddr[op.cli.addr.String()] = op.cli
			close(op.resp)
		case op := <-c.removeClientChan:
			delete(c.clients, op.cli.id)
			delete(c.clientsByAddr, op.cli.addr.String())
			close(op.resp)
		case op := <-c.getClientChan:
			if op.byAddr {
				if cli, ok := c.clientsByAddr[op.addr]; ok {
					op.resp <- cli
				}
				close(op.resp)
			} else {
				if cli, ok := c.clients[op.id]; ok {
					op.resp <- cli
				}
				close(op.resp)
			}
		case op := <-c.rangeClientChan:
			for _, cli := range c.clients {
				op.resp <- cli
			}
			close(op.resp)
		case id := <-c.updatePingChan:
			if cli, ok := c.clients[id]; ok {
				cli2 := cli
				cli2.lastPing = time.Now()
				c.clients[id] = cli2
				c.clientsByAddr[cli.addr.String()] = cli2
			}
		case <-done:
			return
		}
	}
}

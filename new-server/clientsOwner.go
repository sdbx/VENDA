package main

import "time"

type addClientOp struct {
	cli client
}

type removeClientOp struct {
	cli client
}

type getClientOp struct {
	byAddr bool
	addr   string
	id     string
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
	updatePingChan   chan string

	clients       map[string]client
	clientsByAddr map[string]client
}

func newClientsOwner() *clientsOwner {
	return &clientsOwner{
		addClientChan:    make(chan addClientOp),
		removeClientChan: make(chan removeClientOp),
		getClientChan:    make(chan getClientOp),
		rangeClientChan:  make(chan rangeClientOp),
		updatePingChan:   make(chan string),
		clients:          make(map[string]client),
		clientsByAddr:    make(map[string]client),
	}
}

func (c *clientsOwner) AddClient(cli client) {
	c.addClientChan <- addClientOp{
		cli: cli,
	}
}

func (c *clientsOwner) RemoveClient(cli client) {
	c.removeClientChan <- removeClientOp{
		cli: cli,
	}
}

func (c *clientsOwner) GetClientByID(id string) <-chan client {
	out := make(chan client)
	c.getClientChan <- getClientOp{
		byAddr: false,
		id:     id,
		resp:   out,
	}
	return out
}

func (c *clientsOwner) GetClientByAddr(addr string) <-chan client {
	out := make(chan client)
	c.getClientChan <- getClientOp{
		byAddr: true,
		addr:   addr,
		resp:   out,
	}
	return out
}

func (c *clientsOwner) RangeClient() <-chan client {
	out := make(chan client)
	c.rangeClientChan <- rangeClientOp{
		resp: out,
	}
	return out
}

func (c *clientsOwner) UpdatePing(id string) {
	c.updatePingChan <- id
}

func (c *clientsOwner) Run(done <-chan bool) {
	for {
		select {
		case op := <-c.addClientChan:
			c.clients[op.cli.id] = op.cli
			c.clients[op.cli.addr.String()] = op.cli
		case op := <-c.removeClientChan:
			delete(c.clients, op.cli.id)
			delete(c.clients, op.cli.addr.String())
		case op := <-c.getClientChan:
			if op.byAddr {
				if cli, ok := c.clients[op.addr]; ok {
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
				cli.lastPing = time.Now()
				c.clients[id] = cli
			}
		case <-done:
			return
		}
	}
}

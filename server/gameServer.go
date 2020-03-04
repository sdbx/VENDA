package main

import (
	"encoding/json"
	"sync"
	"time"
)

type M map[string]interface{}

type GameServer struct {
	mu       sync.RWMutex
	userData map[string]M
	serv     *Server
}

func NewGameServer(serv *Server) *GameServer {
	return &GameServer{
		serv:     serv,
		userData: make(map[string]M),
	}
}

func (g *GameServer) RegisterHandlers() {
	g.serv.OnConnect(func(id string) {
		g.EmitTo(id, "info", M{"name": GenerateName(), "id": id})
	})
	g.serv.On("animate", wrapFunc(func(id string, buf M) error {
		g.Broadcast("animate", M{"id": buf["id"], "animeId": buf["animeId"]})
		return nil
	}))
	g.serv.On("hit", wrapFunc(func(id string, buf M) error {
		g.EmitTo(buf["target"].(string), "hit", M{"dmg": buf["dmg"], "id": id})
		return nil
	}))
	g.serv.On("death", wrapFunc(func(id string, buf M) error {
		g.Broadcast("death", M{"id": id, "by": buf["id"]})
		return nil
	}))
	g.serv.OnDisconnect(func(id string) {
		g.mu.Lock()
		delete(g.userData, id)
		g.mu.Unlock()
		g.Broadcast("delPlayer", M{"id": id})

	})
	g.serv.On("myData", wrapFunc(func(id string, buf M) error {
		g.mu.Lock()
		g.userData[id] = buf
		g.mu.Unlock()
		return nil
	}))
}

func wrapFunc(fn func(id string, buf M) error) func(string, string) error {
	return func(id string, buf string) error {
		obj := M{}
		err := json.Unmarshal([]byte(buf), &obj)
		if err != nil {
			return err
		}
		return fn(id, obj)
	}
}

func (g *GameServer) EmitTo(id string, event string, payload interface{}) {
	buf, err := json.Marshal(payload)
	if err != nil {
		panic(err)
	}
	g.serv.EmitTo(id, event, string(buf))
}

func (g *GameServer) Broadcast(event string, payload interface{}) {
	buf, err := json.Marshal(payload)
	if err != nil {
		panic(err)
	}
	g.serv.Broadcast(event, string(buf))
}

func (g *GameServer) Run() {
	go func() {
		ticker := time.NewTicker(time.Duration(1000/60) * time.Millisecond)
		for {
			select {
			case <-ticker.C:
				g.mu.RLock()
				for key, data := range g.userData {
					data["id"] = key
					g.Broadcast("userData", data)
				}

				g.mu.RUnlock()
			}
		}
	}()
	g.serv.Run()
}

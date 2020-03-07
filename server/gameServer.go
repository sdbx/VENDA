package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"sync"
	"time"
)

type M map[string]interface{}

type GameServer struct {
	mu        sync.RWMutex
	isBench   bool
	benchInfo []M
	bench     []map[string]time.Time
	packets   map[string][]time.Time
	userData  map[string]M
	serv      *Server
}

func NewGameServer(serv *Server) *GameServer {
	return &GameServer{
		serv:      serv,
		isBench:   false,
		packets:   make(map[string][]time.Time),
		bench:     []map[string]time.Time{},
		benchInfo: []M{},
		userData:  make(map[string]M),
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
		if g.isBench {
			g.packets[id] = append(g.packets[id], time.Now())
		}
		g.userData[id] = buf
		g.mu.Unlock()
		return nil
	}))
	if g.isBench {
		g.serv.On("bench", wrapFunc(func(id string, buf M) error {
			g.mu.Lock()
			g.bench[int(buf["benchId"].(float64))][id] = time.Now()
			g.mu.Unlock()
			return nil
		}))
	}
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
		ticker2 := time.NewTicker(30000 * time.Millisecond)
		for {
			select {
			case <-ticker.C:
				g.mu.Lock()
				for key, data := range g.userData {
					if g.isBench {
						benchId := len(g.bench)
						g.benchInfo = append(g.benchInfo, M{
							"base":     time.Now(),
							"nClients": len(g.userData),
						})
						g.bench = append(g.bench, make(map[string]time.Time))
						data["benchId"] = benchId
					}
					data["id"] = key
					g.Broadcast("userData", data)
				}
				g.mu.Unlock()
			case <-ticker2.C:
				if g.isBench {
					go func() {
						g.mu.RLock()
						fmt.Println("Writing")
						out := []M{}
						for i := range g.bench {
							if time.Now().Sub(g.benchInfo[i]["base"].(time.Time)) < time.Second {
								continue
							}
							lats := make([]float64, 0, len(g.bench[i]))
							for id := range g.bench[i] {
								lats = append(lats, float64((g.bench[i][id].Sub(g.benchInfo[i]["base"].(time.Time)) / time.Millisecond)))
							}
							var total float64 = 0
							var minLat float64 = 999999999
							var maxLat float64 = -999999999
							for _, value := range lats {
								total += float64(value)
								if minLat > value {
									minLat = value
								}
								if maxLat < value {
									maxLat = value
								}
							}
							avg := total
							if float64(len(lats)) != 0 {
								avg /= float64(len(lats))
							}
							complete := float64(len(g.bench[i])) / float64(g.benchInfo[i]["nClients"].(int))
							out = append(out, M{
								"id":            i,
								"minLat":        minLat,
								"maxLat":        maxLat,
								"avgLat":        avg,
								"complete":      complete,
								"numberClients": g.benchInfo[i]["nClients"],
							})
						}
						out2 := []M{}
						for id := range g.packets {
							for _, t := range g.packets[id] {
								out2 = append(out2, M{
									"id":   id,
									"time": t.Unix(),
								})
							}
						}
						buf, err := json.Marshal(out)
						if err != nil {
							fmt.Println(err)
							return
						}
						buf2, err := json.Marshal(out2)
						if err != nil {
							fmt.Println(err)
							return
						}
						ioutil.WriteFile("latency.json", buf, 0744)
						ioutil.WriteFile("packet.json", buf2, 0744)
						g.mu.RUnlock()
					}()
				}
			}
		}
	}()
	g.serv.Run()
}

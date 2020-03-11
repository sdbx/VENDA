package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"server/msgs"
	"sync"
	"time"

	"github.com/golang/protobuf/proto"
)

const (
	EventInfo byte = iota + 1
	EventUserData
	EventMyData
	EventHit
	EventDeath
	EventAnimate
	EventDisconnected
)

type M map[string]interface{}

type GameServer struct {
	mu        sync.RWMutex
	isBench   bool
	benchInfo []M
	bench     []map[string]time.Time
	packets   map[string][]time.Time
	userData  map[int]*msgs.CharacterData
	serv      *Server
}

func NewGameServer(serv *Server) *GameServer {
	return &GameServer{
		serv:      serv,
		isBench:   false,
		packets:   make(map[string][]time.Time),
		bench:     []map[string]time.Time{},
		benchInfo: []M{},
		userData:  make(map[int]*msgs.CharacterData),
	}
}

func (g *GameServer) RegisterHandlers() {
	g.serv.OnConnect(func(id int) {
		out := &msgs.InfoEvent{
			Name: GenerateName(),
			Id:   int32(id),
		}
		buf, _ := proto.Marshal(out)
		g.serv.EmitTo(id, EventInfo, buf)
	})
	g.serv.On(EventAnimate, func(id int, buf []byte) error {
		event := &msgs.AnimateEvent{}
		err := proto.Unmarshal(buf, event)
		if err != nil {
			return err
		}
		event.Id = int32(id)
		buf, err = proto.Marshal(event)
		if err != nil {
			return err
		}
		g.serv.Broadcast(EventAnimate, buf)
		return nil
	})
	g.serv.On(EventHit, func(id int, buf []byte) error {
		event := &msgs.HitEvent{}
		err := proto.Unmarshal(buf, event)
		if err != nil {
			return err
		}
		event.Id = int32(id)
		buf, err = proto.Marshal(event)
		if err != nil {
			return err
		}
		g.serv.EmitTo(int(event.Target), EventHit, buf)
		return nil
	})
	g.serv.On(EventDeath, func(id int, buf []byte) error {
		event := &msgs.DeathEvent{}
		err := proto.Unmarshal(buf, event)
		if err != nil {
			return err
		}
		event.Id = int32(id)
		buf, err = proto.Marshal(event)
		if err != nil {
			return err
		}
		g.serv.Broadcast(EventDeath, buf)
		return nil
	})
	g.serv.OnDisconnect(func(id int) {
		g.mu.Lock()
		delete(g.userData, id)
		g.mu.Unlock()
	})
	g.serv.On(EventMyData, func(id int, buf []byte) error {
		g.mu.Lock()
		// if g.isBench {
		// 	g.packets[id] = append(g.packets[id], time.Now())
		// }
		e := &msgs.MyDataEvent{}
		err := proto.Unmarshal(buf, e)
		if err != nil {
			g.mu.Unlock()
			return err
		}
		g.userData[id] = e.Data
		g.mu.Unlock()
		return nil
	})
	// if g.isBench {
	// 	g.serv.On("bench", wrapFunc(func(id string, buf M) error {
	// 		g.mu.Lock()
	// 		g.bench[int(buf["benchId"].(float64))][id] = time.Now()
	// 		g.mu.Unlock()
	// 		return nil
	// 	}))
	// }
}

func (g *GameServer) Run() {
	go func() {
		// t := time.Now()
		i := 0
		ticker := time.NewTicker(5 * time.Millisecond)
		ticker2 := time.NewTicker(30000 * time.Millisecond)
		for {
			select {
			case <-ticker.C:
				if i < 2 {
					i++
					continue
				}
				i = 0
				// log.Println(time.Now().Sub(t).Milliseconds())
				// t = time.Now()
				g.mu.Lock()
				out := make([]*msgs.CharacterData, 0, len(g.userData))
				for _, data := range g.userData {
					out = append(out, data)
				}

				buf, err := proto.Marshal(&msgs.UserDataEvent{Users: out})
				if err != nil {
					fmt.Println(err)
				}
				g.serv.Broadcast(EventUserData, buf)
				/*
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

					}*/
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

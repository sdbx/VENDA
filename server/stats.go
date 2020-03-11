package main

import (
	"fmt"
	"math"
	"strings"
	"sync"

	"github.com/montanaflynn/stats"
)

type resetter struct {
	mu         sync.Mutex
	Name       string
	Value      int
	ResetCount int
	Target     int
}

func (r *resetter) Describe() string {
	r.mu.Lock()
	out := fmt.Sprintf("%s: %d", r.Name, r.Value)
	r.mu.Unlock()
	return out
}

type statsData struct {
	mu       sync.Mutex
	Name     string
	Value    []float64
	Cap      int
	Counting int
	K        bool
}

func (s *statsData) append(data float64) {
	s.mu.Lock()
	s.Value = appendQueue(s.Value, data, s.Cap)
	s.mu.Unlock()
}

func (s *statsData) count(data int) {
	s.mu.Lock()
	s.Counting += data
	s.mu.Unlock()
}

func (s *statsData) flush() {
	s.append(float64(s.Counting))
	s.mu.Lock()
	s.Counting = 0
	s.mu.Unlock()
}

type Stats struct {
	nClients          *statsData
	nRecvBytes        *statsData
	nSentBytes        *statsData
	nRecvPackets      *statsData
	nSentPackets      *statsData
	nEvent            *statsData
	nMsg              *statsData
	nDone             *statsData
	clientsOwnerQueue *statsData
	sendChanQueue     *statsData
	msgChanQueue      *statsData
	eventChanQueue    *statsData
	nGarbage          *resetter
}

func newStatsData(name string, cap int) *statsData {
	return &statsData{
		Name:     name,
		Value:    []float64{},
		Cap:      cap,
		Counting: 0,
	}
}

func newResetter(name string, target int) *resetter {
	return &resetter{
		Name:       name,
		Target:     target,
		ResetCount: 0,
		Value:      0,
	}
}

func (s *statsData) Describe(dt float64) string {
	s.mu.Lock()
	med, _ := stats.Median(s.Value)
	iqr, _ := stats.InterQuartileRange(s.Value)
	mean, err := stats.Mean(s.Value)
	if err != nil {
		panic(err)
	}
	sd, err := stats.StandardDeviation(s.Value)
	if err != nil {
		panic(err)
	}
	rate := 0.0
	ac := 0.0
	if len(s.Value) >= s.Cap {
		cand1, _ := stats.AutoCorrelation(s.Value, statsCap/100)
		cand2, _ := stats.AutoCorrelation(s.Value, statsCap/50)
		cand3, _ := stats.AutoCorrelation(s.Value, statsCap/10)
		ac = math.Max(math.Max(cand1, cand2), cand3)
		rate = (s.Value[s.Cap-1] - s.Value[0]) / statsCap
	}
	cur := 0.0
	if len(s.Value) != 0 {
		cur = s.Value[len(s.Value)-1]
	}

	s.mu.Unlock()
	return fmt.Sprintf("%s: %.3f (mean: %.3f sd: %.3f median: %.3f iqr: %.3f rate: %.3f ac: %.3f)", s.Name, cur*1000.0/dt, mean*1000.0/dt, sd*1000.0/dt, med/dt*1000.0, iqr/dt*1000.0, rate*1000.0, ac)
}

const statsCap = 1000

func NewStats() *Stats {
	return &Stats{
		nClients:          newStatsData("# of clients", statsCap),
		nRecvBytes:        newStatsData("recv bytes", statsCap),
		nSentBytes:        newStatsData("sent bytes", statsCap),
		nRecvPackets:      newStatsData("recv #", statsCap),
		nSentPackets:      newStatsData("sent #", statsCap),
		nEvent:            newStatsData("processed msg #", statsCap),
		nMsg:              newStatsData("recv msg #", statsCap),
		nDone:             newStatsData("processed event #", statsCap),
		clientsOwnerQueue: newStatsData("pending clients owner", statsCap),
		sendChanQueue:     newStatsData("pending send channel", statsCap),
		msgChanQueue:      newStatsData("pending message channel", statsCap),
		eventChanQueue:    newStatsData("pending event channel", statsCap),
		nGarbage:          newResetter("# of garbages dropped", 1000),
	}
}

func (s *Stats) Describe(dt float64) string {
	return strings.Join([]string{
		s.nClients.Describe(1000.0),
		s.nRecvBytes.Describe(dt),
		s.nSentBytes.Describe(dt),
		s.nRecvPackets.Describe(dt),
		s.nSentPackets.Describe(dt),
		s.nEvent.Describe(1000.0),
		s.nMsg.Describe(1000.0),
		s.nDone.Describe(1000.0),
		s.clientsOwnerQueue.Describe(1000.0),
		s.sendChanQueue.Describe(1000.0),
		s.msgChanQueue.Describe(1000.0),
		s.eventChanQueue.Describe(1000.0),
		s.nGarbage.Describe(),
	}, "\n")
}

func (r *resetter) reset() {
	r.mu.Lock()
	r.ResetCount++
	if r.ResetCount >= r.Target {
		r.Value = 0
	}
	r.mu.Unlock()
}

func appendQueue(slice []float64, point float64, cap int) []float64 {
	if len(slice) < cap {
		slice = append(slice, point)
	} else {
		slice = append(slice[1:], point)
	}
	return slice
}

func (s *Stats) Monitor(serv *Server) {
	owner := serv.clientsSliceOwner
	co := float64(len(owner.addClientChan) + len(owner.removeClientChan) + len(owner.getClientChan) +
		len(owner.rangeClientChan) +
		len(owner.updatePingChan))
	s.clientsOwnerQueue.append(co)
	s.nRecvBytes.flush()
	s.nSentBytes.flush()
	s.nRecvPackets.flush()
	s.nSentPackets.flush()
	s.sendChanQueue.append(float64(len(serv.sendChan)))
	s.msgChanQueue.append(float64(len(serv.messageChan)))
	s.eventChanQueue.append(float64(len(serv.eventChan)))
	s.nClients.append(float64(len(serv.clientsSliceOwner.clients)))
	s.nEvent.flush()
	s.nDone.flush()
	s.nMsg.flush()
	s.nGarbage.reset()
}

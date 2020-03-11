package main

import (
	"flag"
	"math"
	"time"

	tm "github.com/buger/goterm"
)

var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to file")

func debugDisplay(serv *Server) {
	t := time.NewTicker(500 * time.Millisecond)
	for {
		select {
		case <-t.C:
			tm.Clear()
			tm.MoveCursor(0, 0)
			tm.Println(time.Now())
			// chart := tm.NewLineChart(50, 20)
			data := new(tm.DataTable)
			data.AddColumn("Time")
			data.AddColumn("Sin(x)")
			data.AddColumn("Cos(x+1)")

			for i := 0.1; i < 10; i += 0.1 {
				data.AddRow(i, math.Sin(i), math.Cos(i+1))
			}

			tm.Println(serv.st.Describe(10.0))

			tm.Flush() // Call it every time at the end of rendering

		}
	}

}

func main() {
	server := NewServer(":5858")
	gserver := NewGameServer(server)
	gserver.RegisterHandlers()
	go debugDisplay(server)
	gserver.Run()
}

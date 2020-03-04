package main

func main() {
	server := NewServer(":5859")
	gserver := NewGameServer(server)
	gserver.RegisterHandlers()
	gserver.Run()
}

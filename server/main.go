package main

func main() {
	server := NewServer(":5858")
	gserver := NewGameServer(server)
	gserver.RegisterHandlers()
	gserver.Run()
}

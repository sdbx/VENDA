package main

func main() {
	server := NewServer(":5858")
	server.Run()
}

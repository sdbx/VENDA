package main

import "fmt"

func main() {
	server := NewServer(":5858")
	server.On("asdf", func(buf string) error {
		fmt.Println(buf)
		return nil
	})
	server.Run()
}

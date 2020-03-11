PATH="$PATH:$HOME/go/bin"
./protoc -I=./proto --go_out=. ./proto/msg.proto
./protoc -I=./proto --csharp_out=./client/Assets/Proto ./proto/msg.proto

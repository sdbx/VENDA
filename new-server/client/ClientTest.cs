using System;

using System.Threading;

namespace Amguna {
    class Program {
        static void Main(string[] args) {
            var socket = new EventSocket("127.0.0.1:5355");
            socket.Emit("death", "Asdf");
            Thread.Sleep(2000);
            socket.Emit("death", "Asdf");
            socket.Emit("death", "Asdf");
            socket.Emit("death", "Asdf");
            Thread.Sleep(2000);
            socket.Emit("death", "Asdf");
            socket.Emit("death", "Asdf");
            socket.Emit("death", "Asdf");
        }
    }
}

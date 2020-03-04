using System;

using System.Threading;

namespace Amguna {
    class Program {
        static void Main(string[] args) {
            var socket = new EventSocket("127.0.0.1:5859");
            while (true) {
                socket.Update();
                socket.Emit("asdf", "{}");
                Thread.Sleep(100);
            }
        }
    }
}

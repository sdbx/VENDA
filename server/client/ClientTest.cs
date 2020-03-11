using System;

using System.Threading;

namespace Amguna {
    class Program {
        static void Main(string[] args) {
            var socket = new EventSocket("15.164.88.75:5858");
            socket.On("userData", (string str) => {
                Console.WriteLine(System.Text.UTF8Encoding.Unicode.GetByteCount(str));
            });
            while (true) {
                socket.Update(); 
                socket.Emit("asdf", "{}");
                Thread.Sleep(17);
            }
        }
    }
}

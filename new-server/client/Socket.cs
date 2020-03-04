using System;  
using System.Net;  
using System.Net.Sockets;
using System.Globalization;  
using System.Threading;  
using System.Text;  

namespace Amguna {

    class EventSocket {
        public ManualResetEvent connectDone =   
            new ManualResetEvent(false);
        public ManualResetEvent receiveDone =   
            new ManualResetEvent(false); 
        private Socket _socket;
        private IPEndPoint _remoteEP;
        private string _addr;

        private static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if(ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if(!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            int port;
            if(!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public EventSocket(string addr) {
            _addr = addr;
            _remoteEP = CreateIPEndPoint(_addr);
            _socket = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp); 
           
        }

        private void ConnectCallback(IAsyncResult ar) {  
            try {    
                Socket client = (Socket) ar.AsyncState;  
    
                // Complete the connection.  
                client.EndConnect(ar);  
    
                Console.WriteLine("Socket connected to {0}",  
                    client.RemoteEndPoint.ToString());  
    
                // Signal that the connection has been made.  
                connectDone.Set();  
            } catch (Exception e) {  
                Console.WriteLine(e.ToString());  
            }  
        } 

        public void Emit(string eventName, string data) {
            byte[] byteData = Encoding.UTF8.GetBytes(eventName + ":" + data);  
  
            // Begin sending the data to the remote device.  
            _socket.SendTo(byteData, 0, byteData.Length, 0, _remoteEP);
        }
        public void On(string eventName, Action<string> callback) {

        }
    }
}

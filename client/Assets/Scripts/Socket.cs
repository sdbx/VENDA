using System;  
using System.Net;  
using System.Net.Sockets;
using System.Globalization;  
using System.Threading;  
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using UnityEngine;

namespace Amguna {
    public class ReceivedEventArgs : EventArgs  
    {  
        public ReceivedEventArgs(string s)  
        {  
            msg = s;  
        }  
        public string msg; 
    }  

    public class EventSocket {
        private ConcurrentQueue<ReceivedEventArgs> queue = new ConcurrentQueue<ReceivedEventArgs>();
        private UdpClient _client;
        private IPEndPoint _remoteEP;
        private string _addr;

        private string _id = null;
        private byte[] _secret = null;

        private Dictionary<string, Action<string>> _handlers = new Dictionary<string, Action<string>>();
        private Thread _th;

        private static void Log(string msg) {
            Debug.Log(msg);
        }

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
            _client = new UdpClient();
            _client.Client.ReceiveBufferSize = 1024;
            _connect();

            _th = new Thread(Receive);
            _th.IsBackground = true;  
            _th.Start();
        }
        
        private void _connect() {
            if (_id == null) {
                SendHandshake();
                Log("sent Handshake");
            }
        }

        private void Receive() {  
            while (true)
            {
                try {
                    var receivedResults = _client.Receive(ref _remoteEP);
                    //Debug.Log(Encoding.UTF8.GetString(receivedResults));
                    queue.Enqueue(new ReceivedEventArgs(Encoding.UTF8.GetString(receivedResults)));
                } catch (Exception e) {
                    Log($"ERROR {e.ToString()}");
                }
                
            }
        }
        
        public void Disconnect() {
            byte[] byteData = Combine(new byte[] {0x03}, _secret);
            _client.Send(byteData, byteData.Length, _remoteEP);
        }

        private void SendHandshake() {
            byte[] byteData = new byte[] {0x01};
            _client.Send(byteData, byteData.Length, _remoteEP);
        }

        private byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];    
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }


        public void Emit(string eventName, string data) {

            if (_id != null) {
                byte[] byteData = Combine(Combine(new byte[] {0x02}, _secret), Encoding.UTF8.GetBytes(eventName + ":" + data));
                _client.Send(byteData, byteData.Length, _remoteEP);
            }
        }
        public void On(string eventName, Action<string> callback) {
            _handlers[eventName] = callback;
        }

        private void ReceiveEvent(ReceivedEventArgs eventArgs) {
            try {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(eventArgs.msg);
                if (data["event"] == "connected") {
                    var payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(data["payload"]);
                    _id = payload["id"];
                    _secret = System.Convert.FromBase64String(payload["secret"]);
                    Log($"CONNECTED as {_id}");
                }

                if ( _handlers.ContainsKey(data["event"])) {
                    _handlers[data["event"]](data["payload"]);
                }
            } catch (Exception e) {
                Log($"ERROR {e.ToString()} ");
            }
        }

        public void Update() {
            ReceivedEventArgs eventArgs;
            Log(queue.Count+"");
            while (queue.TryDequeue(out eventArgs)) {
                ReceiveEvent(eventArgs);
            }
        }
    }
}
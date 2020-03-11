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
using System.Linq;
using UnityEngine;

namespace Amguna {
    public class ReceivedEventArgs : EventArgs  
    {  
        public ReceivedEventArgs(byte[] s)  
        {  
            msg = s;  
        }  
        public byte[] msg; 
    }  

    public class EventSocket {
        private ConcurrentQueue<ReceivedEventArgs> queue = new ConcurrentQueue<ReceivedEventArgs>();
        private UdpClient _client;
        private IPEndPoint _remoteEP;
        private string _addr;

        private int _id = -1;
        private byte[] _secret = null;

        private Dictionary<byte, Action<byte[]>> _handlers = new Dictionary<byte, Action<byte[]>>();

        public Action<int> OnConnect;

        public Action<int> OnDisconnect;
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
            if (_id == -1) {
                SendHandshake();
                Log("sent Handshake");
            }
        }

        private void Receive() {  
            while (true)
            {
                try {
                    var receivedResults = _client.Receive(ref _remoteEP);
                    queue.Enqueue(new ReceivedEventArgs(receivedResults));
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


        public void Emit(byte eventByte, byte[] data) {
            if (_id != -1) {
                byte[] byteData = Combine(Combine(Combine(new byte[] {0x02}, _secret), new byte[] {eventByte}), data);
                _client.Send(byteData, byteData.Length, _remoteEP);
            }
        }
        public void On(byte eventByte, Action<byte[]> callback) {
            _handlers[eventByte] = callback;
        }

        private void ReceiveEvent(ReceivedEventArgs eventArgs) {
            try {
                var type = eventArgs.msg[0];
                var iter = eventArgs.msg.Skip(1);
                switch (eventArgs.msg[0]) {
                    case 0x01:
                        _id = (int)BitConverter.ToInt32(iter.Take(4).ToArray(), 0);
                        iter = iter.Skip(4);
                        _secret = iter.Take(16).ToArray();
                        Log($"CONNECTED as {_id}");
                        OnConnect(_id);
                        break;
                    case 0x02:
                        var eventByte = iter.Take(1).ToArray()[0];
                        var buffer = iter.Skip(1).ToArray();
                        if ( _handlers.ContainsKey(eventByte)) {
                            _handlers[eventByte](buffer);
                        }
                        break;
                    
                    case 0x03:
                        var id = BitConverter.ToInt32(iter.Take(4).ToArray(), 0);
                        OnDisconnect(id);
                        break;
                }
            } catch (Exception e) {
                Log($"ERROR {e.ToString()} ");
                // Log(eventArgs.msg.ToString());
            }
        }

        public void Update() {
            ReceivedEventArgs eventArgs;
            while (queue.TryDequeue(out eventArgs)) {
                ReceiveEvent(eventArgs);
            }
        }
    }
}
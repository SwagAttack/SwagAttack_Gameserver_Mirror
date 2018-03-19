using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageControl.Interfaces;

namespace MessageControl.Controllers
{
    public class AsyncData
    {
        // Socket for the client
        public TcpClient Client = null;
        // Buffer size
        public const int BuffSize = 1024;
        // Recieve buff
        public byte[] RecvBuff = new byte[BuffSize];
        // Received data string.  
        public StringBuilder Sb = new StringBuilder();
    }
    public class Input : IInput
    {
        public event ResponseDel InputEvent;
        public event ErrorDel ErrorEvent;

        private readonly ManualResetEvent _doneEvent = new ManualResetEvent(false);

        private readonly Thread _inputThread;
        private readonly TcpListener _listener;

        public Input(string ip, short port)
        {
            _inputThread = new Thread(Listen) {IsBackground = true};
            _listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            //_inputThread.Start();  
            Listen();
        }

        private void Listen()
        {
            try
            {
                _listener.Start(100);

                while (true)
                {
                    /* No longer waiting for a connection */
                    _doneEvent.Reset();

                    Console.WriteLine("Waiting for a connection...");

                    /* Start listening asynchronous */
                    _listener.BeginAcceptTcpClient(AcceptCallBack, _listener);

                    /* Wait till a connection is made before proceeding */
                    _doneEvent.WaitOne();
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                _listener.Stop();
            }
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            /* For signalling the listening thread to continue */
            _doneEvent.Set();

            /* Accept the client */
            var client = _listener.EndAcceptTcpClient(ar);

            /* Begin reading from the client */
            var data = new AsyncData {Client = client};
            var clientStream = client.GetStream();
            clientStream.BeginRead(data.RecvBuff, 0, AsyncData.BuffSize, ReadCallBack, data);        
        }

        public void ReadCallBack(IAsyncResult ar)
        {
            var data = (AsyncData) ar.AsyncState;
            var client = data.Client;

            var clientStream = client.GetStream();
            var bytesRead = clientStream.EndRead(ar);

            if (bytesRead > 0)
            {
                data.Sb.Append(Encoding.ASCII.GetString(data.RecvBuff, 0, bytesRead));
                var received = data.Sb.ToString();

                if (ProtocolConverter.IsEndOfSequence(received))
                {
                    Console.WriteLine($"Read: {received}");

                    var key = ProtocolConverter.ConvertToKey(received);
                    var val = ProtocolConverter.ConvertToValue(received);
                    
                    var handler = InputEvent?.Invoke(key);
                    if (handler != null)
                    {
                        var response = handler(key, val);
                        Console.WriteLine($"Response: {response}");
                    }

                    client.Close();
                    
                }
                else
                {
                    clientStream.BeginRead(data.RecvBuff, 0, AsyncData.BuffSize, ReadCallBack, data);
                }
            }
            else
            {
                client.Close();
            }

        }

    }

    public class InputException : Exception
    {
        public InputException() { }
        public InputException(string message) : base(message) { }
        public InputException(string message, Exception inner) : base(message, inner) { }
    }
}
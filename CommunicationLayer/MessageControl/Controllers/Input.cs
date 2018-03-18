using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MessageControl.Interfaces;

namespace MessageControl.Controllers
{
    internal class AsyncData
    {
        // Socket for the client
        public Socket WorkSocket = null;
        // Buffer size
        public const int BuffSize = 1024;
        // Recieve buff
        public byte[] RecvBuff = new byte[BuffSize];
        // Received data string.  
        public StringBuilder Sb = new StringBuilder();
    }
    public class Input : IInput
    {
        public event InputDeletegate InputEvent;

        private ManualResetEvent _doneEvent = new ManualResetEvent(false);

        private readonly Thread _inputThread;

        public Input()
        {
            _inputThread = new Thread(Listen);
        }

        private void Listen()
        {

        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            var listener = (Socket) ar.AsyncState;
            var client = listener.EndAccept(ar);
        }
    }
}
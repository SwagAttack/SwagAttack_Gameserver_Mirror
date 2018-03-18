using System.Collections.Generic;
using System.Linq;
using MessageControl.DTO;
using MessageControl.Interfaces;

namespace MessageControl.Controllers
{
    public class MessageSystem : IMessageSystem
    {
        private readonly Dictionary<string, HashSet<MessageCommand>> _subscriberCommands;

        private readonly IMessageConverter _msgConverter;
        private readonly IInputOutputControl _inputOutputControl;

        public MessageSystem(IInputOutputControl inputOutputControl, IMessageConverter msgConverter)
        {
            _inputOutputControl = inputOutputControl;
            _inputOutputControl.Input.InputEvent += Notify;

            _msgConverter = msgConverter;

            _subscriberCommands = new Dictionary<string, HashSet<MessageCommand>>();
        }

        public bool Attach(string id, MessageCommand cmd)
        {
            lock (_subscriberCommands)
            {
                if (!_subscriberCommands.ContainsKey(id))
                {
                    _subscriberCommands.Add(id, new HashSet<MessageCommand>());
                }

                return _subscriberCommands[id].Add(cmd);
            }
        }

        public bool Detatch(string id, MessageCommand cmd)
        {
            lock (_subscriberCommands)
            {
                return _subscriberCommands.ContainsKey(id) && _subscriberCommands[id].Remove(cmd);
            }
        }

        public void Notify(string inputkey, string input)
        {
            lock (_subscriberCommands)
            {
                if (!_subscriberCommands.ContainsKey(input)) return;

                var msg = _msgConverter.ConvertToInput(inputkey, input);

                if (msg == null) return;

                foreach (var cmd in _subscriberCommands[inputkey])
                {
                    cmd(msg);
                }
            }
        }

        public void SendMessage(IMessage msg)
        {
            var msgToSend = _msgConverter.ConvertToOutput(msg);
            if (msgToSend != null)
            {
                _inputOutputControl.Output.SendMessage(msgToSend.Item1, msgToSend.Item2);
            }         
        }
    }
}
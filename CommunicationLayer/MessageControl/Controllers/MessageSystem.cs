using System.Collections.Generic;
using System.Linq;
using MessageControl.DTO;
using MessageControl.Interfaces;

namespace MessageControl.Controllers
{
    public class MessageSystem : IMessageSystem
    {
        private readonly Dictionary<string, HandlerDelegate> _subscriberCommands;

        private readonly IInput _input;

        public MessageSystem(IInput input)
        {
            _input = input;

            _input.InputEvent += FindSubscriberDel;

            _subscriberCommands = new Dictionary<string, HandlerDelegate>();
        }

        public bool Attach(string id, HandlerDelegate cmd)
        {
            lock (_subscriberCommands)
            {
                if (!_subscriberCommands.ContainsKey(id))
                {
                    _subscriberCommands.Add(id, cmd);
                    return true;
                }

                return false;
            }
        }

        public bool Detatch(string id, HandlerDelegate cmd)
        {
            lock (_subscriberCommands)
            {
                return _subscriberCommands.Remove(id);
            }
        }

        public HandlerDelegate FindSubscriberDel(string id)
        {
            lock (_subscriberCommands)
            {
                if (_subscriberCommands.ContainsKey(id))
                {
                    return _subscriberCommands[id];
                }

                return null;
            }
        }
    }
}
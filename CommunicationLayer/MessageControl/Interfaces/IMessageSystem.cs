using System;
using MessageControl.DTO;

namespace MessageControl.Interfaces
{
    public delegate void MessageCommand(IMessage cmd);
    public interface IMessageSystem
    {
        /*
         * Attaches caller to the message system
         * Caller will be notified on the given ID
         */
        bool Attach(string id, MessageCommand cmd);
        /*
         * Detaches caller from the message system
         */
        bool Detatch(string id, MessageCommand cmd);
        /*
         * For notifying the message system on inputs from input control
         * inputkey : key identifying the data received
         * input : actual data
         */
        void Notify(string inputkey, string input);
        /*
         * For sending a message to the clients of the game server
         */
        void SendMessage(IMessage msg);
    }
}
using System;
using MessageControl.DTO;

namespace MessageControl.Interfaces
{
    public interface IMessageSystem
    {
        /*
         * Attaches caller to the message system
         * Caller will be notified on the given ID
         */
        bool Attach(string id, HandlerDelegate cmd);
        /*
         * Detaches caller from the message system
         */
        bool Detatch(string id, HandlerDelegate cmd);

        /*
         * For retrieving commands coupled to IDs
         */
        HandlerDelegate FindSubscriberDel(string id);
    }
}
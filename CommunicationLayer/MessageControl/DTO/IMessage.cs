namespace MessageControl.DTO
{
    public interface IMessage
    {
        /*
         * Identifies the Message
         */
        string MessageId { get; }  
        /*
         * Contains the actual object of the message. Is identified by Message ID
         */
        object Message { get; }
    }

    
}
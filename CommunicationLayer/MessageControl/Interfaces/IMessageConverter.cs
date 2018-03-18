using System;
using MessageControl.DTO;

namespace MessageControl.Interfaces
{

    public interface IMessageConverter
    {
        /*
         * Will convert the given input input given with the input key to the appropriate IMessage
         * Will return null on error
         */
        IMessage ConvertToInput(string inputkey, string input);
        Tuple<string, string> ConvertToOutput(IMessage ouput);
    }
}
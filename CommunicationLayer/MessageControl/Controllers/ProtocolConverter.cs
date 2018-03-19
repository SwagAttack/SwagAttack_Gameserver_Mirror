using System;
using MessageControl.Interfaces;

namespace MessageControl.Controllers
{
    public static class ProtocolConverter
    {
        public static string[] CommandSeperator = {"<DOPE>"};
        public static string EndOfSequenceSeperator = "<SwagBoy>";
        public static bool IsEndOfSequence(string input)
        {
            return (input.IndexOf(EndOfSequenceSeperator)) > -1;
        }

        public static string ConvertToKey(string input)
        {
            var lastCharIndex = input.IndexOf(EndOfSequenceSeperator);
            if(lastCharIndex == -1)
                throw new ArgumentException("input");

            var keyval = ConvertToKeyVal(input, lastCharIndex);
            return keyval[0].Trim();
        }

        public static string ConvertToValue(string input)
        {
            var lastCharIndex = input.IndexOf(EndOfSequenceSeperator);
            if (lastCharIndex == -1)
                throw new ArgumentException("input");

            var keyval = ConvertToKeyVal(input, lastCharIndex);

            if (keyval.Length > 1)
                return keyval[1].Trim();
            else
                return null;
        }

        private static string[] ConvertToKeyVal(string input, int lastCharIndex)
        {
            return input.Substring(0, lastCharIndex).Split(CommandSeperator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
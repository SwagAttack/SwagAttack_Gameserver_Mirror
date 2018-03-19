namespace MessageControl.Interfaces
{
    public interface IProtocolConverter
    {
        bool IsEndOfSequence(string input);
        string ConvertToKey(string input);
        string ConvertToValue(string input);
    }
}
namespace MessageControl.Interfaces
{
    public delegate void InputDeletegate(string msgkey, string input);
    public interface IInput
    {
        /*
         * For notifiction upon input
         */
        event InputDeletegate InputEvent;
    }
}
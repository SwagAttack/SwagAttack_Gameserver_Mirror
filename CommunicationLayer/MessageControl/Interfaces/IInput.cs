namespace MessageControl.Interfaces
{
    public delegate string HandlerDelegate(string key, string input);

    public delegate HandlerDelegate ResponseDel(string key);

    public delegate void ErrorDel();

    public interface IInput
    {
        /*
         * For notifiction upon input
         */
        event ResponseDel InputEvent;

        /*
         * For notfication upon error
         */
        event ErrorDel ErrorEvent;
    }
}
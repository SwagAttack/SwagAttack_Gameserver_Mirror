namespace MessageControl.Interfaces
{
    public interface IInputOutputControl : IInput, IOutput
    {
        IInput Input { get; }
        IOutput Output { get; }
    }
}
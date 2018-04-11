namespace Application.Interfaces
{
    public interface IManager<T> where T : class
    {
        T GetInstance();
    }
}
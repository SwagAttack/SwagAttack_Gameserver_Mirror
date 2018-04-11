using Models.Interfaces;

namespace Application.Interfaces
{
    public interface ILoginManager : IManager<ILoginManager>
    {
        bool Login(IUser user);
        bool CheckLoginStatus(IUser user);
        void UpdateLoginStatus(IUser user, bool status);
    }
}
using System;
using Models.Interfaces;

namespace Application.Interfaces
{
    public delegate void UserLoggedOutHandle(object obj, string username);
    public interface ILoginManager 
    {
        void Login(IUser user);
        bool CheckLoginStatus(IUser user);
        bool SubscribeOnLogOut(string username, UserLoggedOutHandle handle);
        bool UnsubscribeOnLogOut(string username, UserLoggedOutHandle handle);
    }
}
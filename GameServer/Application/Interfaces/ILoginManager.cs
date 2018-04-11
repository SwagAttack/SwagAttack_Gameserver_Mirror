using System;
using Models.Interfaces;

namespace Application.Interfaces
{
    public interface ILoginManager 
    {
        bool Login(IUser user);
        bool CheckLoginStatus(IUser user);
    }
}
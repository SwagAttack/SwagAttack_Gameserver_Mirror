using Application.Interfaces;
using Models.Interfaces;

namespace Application.Managers
{
    public class LoginManager : ILoginManager
    {
        private static LoginManager _instance = null;
        
        public ILoginManager GetInstance()
        {
            return _instance ?? (_instance = new LoginManager());
        }

        public bool Login(IUser user)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckLoginStatus(IUser user)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateLoginStatus(IUser user, bool status = true)
        {
            throw new System.NotImplementedException();
        }

        private LoginManager()
        {

        }
    }
}
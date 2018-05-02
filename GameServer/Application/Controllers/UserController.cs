using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Controllers
{
    /// <summary>
    /// Application User Controller
    /// The main purpose of this class is to decouple the framework from our application logic
    /// </summary>
    public class UserController : IUserController
    {
        private readonly ILoginManager _loginManager;
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository, ILoginManager loginManager)
        {
            _userRepository = userRepository;
            _loginManager = loginManager;
        }
        public IUser GetUser(string username, string password)
        {
            var user = _userRepository.GetItemAsync(username).Result;

            if (user != null)
            {
                if (user.Password == password)
                {
                    _loginManager.Login(user);
                    return user;
                }
            }

            return null;
        }

        public IUser CreateUser(IUser user)
        {
            var result = _userRepository.CreateItemAsync(user).Result;

            if (result != null)
            {
                _loginManager.Login(result);
                return result;
            }

            return null;
        }

        public IUser UpdateUser(string username, IUser user)
        {
            if (username != user.Username)
                return null;

            var result = _userRepository.UpdateItemAsync(username, user).Result;

            if (result != null)
            {
                _loginManager.Login(result);
                return result;
            }

            return null;
        }
    }
}
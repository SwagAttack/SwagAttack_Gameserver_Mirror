using Application.Interfaces;
using Domain.Interfaces;
using Persistance.UnitOfWork;

namespace Application.Controllers
{
    /// <summary>
    /// Application User Controller
    /// The main purpose of this class is to decouple the framework from our application logic
    /// </summary>
    public class UserController : IUserController
    {
        private readonly ILoginManager _loginManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork, ILoginManager loginManager)
        {
            _unitOfWork = unitOfWork;
            _loginManager = loginManager;
        }
        public IUser GetUser(string username, string password)
        {
            var user = _unitOfWork.UserRepository.GetUserByUsername(username);

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
            if (_unitOfWork.UserRepository.GetUserByUsername(user.Username) == null)
            {
                _unitOfWork.UserRepository.AddUser(user);
                _loginManager.Login(user);
                return user;
            }

            return null;
        }

        public IUser UpdateUser(string username, IUser user)
        {
            var result = _unitOfWork.UserRepository.GetUserByUsername(username);

            if (result != null)
            {
                if (username == user.Username)
                {
                    _unitOfWork.UserRepository.ReplaceUser(user);
                    return user;
                }

            }

            return null;
        }
    }
}
using Application.Interfaces;
using DBInterface.UnitOfWork;
using Models.Interfaces;

namespace Application.Controllers
{
    /// <summary>
    /// Application User Controller
    /// The main purpose of this class is to decouple the framework from our application logic
    /// </summary>
    public class UserController : IUserController
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }
        public IUser GetUser(string username, string password)
        {
            var user = _unitOfWork.UserRepository.GetUserByUsername(username);

            if (user != null)
            {
                if (user.Password == password)
                {
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
                return user;
            }

            return null;
        }

        public IUser UpdateUser(string username, string password, IUser user)
        {
            var result = _unitOfWork.UserRepository.GetUserByUsername(username);

            if (result != null)
            {
                if (result.Password == password)
                {
                    //result.Username = user.Username;
                    result.Password = user.Password;
                    result.Email = user.Email;
                    result.GivenName = user.GivenName;
                    result.LastName = user.LastName;

                    _unitOfWork.UserRepository.ReplaceUser(user);

                    return result;
                }
            }

            return null;
        }
    }
}
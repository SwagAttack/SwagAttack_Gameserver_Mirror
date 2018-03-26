using System.Linq;
using Models.Interfaces;
using RESTControl.DAL_Simulation;
using RESTControl.Interfaces;

namespace RESTControl.Controllers
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
            var user = _unitOfWork.Users.FirstOrDefault(u => u.Username == username);

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
            if (_unitOfWork.Users.FirstOrDefault(u=> u.Username == user.Username) == null)
            {
                _unitOfWork.Users.Add(user);
                return user;
            }

            return null;
        }

        public IUser UpdateUser(string username, string password, IUser user)
        {
            var result = _unitOfWork.Users.FirstOrDefault(u => u.Username == username);

            if (result != null)
            {
                if (result.Password == password)
                {
                    result.Username = user.Username;
                    result.Password = user.Password;
                    result.Email = user.Email;
                    result.GivenName = user.GivenName;
                    result.LastName = user.LastName;

                    return result;
                }
            }

            return null;
        }
    }
}
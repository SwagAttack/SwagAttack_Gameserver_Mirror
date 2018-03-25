using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBInterface.DAL;
using Models.Interfaces;
using Models.User;

namespace DBInterface.UnitOfWork
{
   
    public class UnitOfWork
    {
        private UserRepository<User> _userRepository;

        public UnitOfWork()
        {
            _userRepository = new UserRepository<User>();

        }

        public void AddUser(User thisUser)
        {
            try
            {
                _userRepository.GetUserById(thisUser.Email).Wait();
                Console.WriteLine("user already exsist!");
            }
            catch (Exception e)
            {
                _userRepository.AddUser(thisUser).Wait();
            }
        }

        public User GetUserById(string id)
        {
            User xy = _userRepository.GetUserById(id).Result;
            return xy;
        }
    }
}

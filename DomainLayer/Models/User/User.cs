using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Interfaces;
using System.ComponentModel.DataAnnotations;
namespace Models.User
{
    public class User : IUser
    {
        private void throwArgumentIf(bool condition, string message)
        {
            if(condition)
                throw new ArgumentException(message);
        }
        private string _username;
        private string _givenName;
        private string _lastName;
        private string _email;
        private string _password;

        public string Username
        {
            get { return _username; }
            set
            {
                throwArgumentIf(string.IsNullOrEmpty(value),"Username cannot be empty!");
                throwArgumentIf(value.Length < 8, "Username cannot be less than 8 characters");

                _username = value;
            }
        }

        public string GivenName
        {
            get { return _givenName; }
            set { _givenName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}

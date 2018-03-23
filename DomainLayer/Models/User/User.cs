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
                ValidateUsername(value);
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

        private void throwArgumentIf(bool condition, string message)
        {
            if (condition)
                throw new ArgumentException(message);
        }

        private void ValidateUsername(string value)
        {
            throwArgumentIf(string.IsNullOrEmpty(value), "value cannot be empty!");
            throwArgumentIf(value.Length < 8, "value cannot be less than 8 characters");
            throwArgumentIf(value.Length > 20, "value cannot be more than 20 characters");
            throwArgumentIf(value.Any(x => !Char.IsLetterOrDigit(x)), "value only be letters from a to z or numbers");
        }
    }
}

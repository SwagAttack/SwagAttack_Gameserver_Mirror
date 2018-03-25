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
                new UsernameThrow().Validate(value);
                _username = value;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                new PasswordThrow().Validate(value);
                _password = value;
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                new EmailThrow().Validate(value);
                _email = value;
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

    }
}

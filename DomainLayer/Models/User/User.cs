using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Interfaces;

namespace Models.User
{
    public class User : IUser
    {
        private string _username;
        private string _givenName;
        private string _surName;
        private string _email;
        private string _password;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string GivenName
        {
            get { return _givenName; }
            set { _givenName = value; }
        }

        public string SurName
        {
            get { return _surName; }
            set { _surName = value; }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Newtonsoft.Json;

namespace Models.User
{
    public class User : IUser
    {
        private string _username;
        private string _givenName;
        private string _lastName;
        private string _email;
        private string _password;

        [JsonProperty(Required = Required.Always, PropertyName = "id")]
        public string Username
        {
            get { return _username; }
            set
            {
                new UsernameThrow().Validate(value);
                _username = value;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public string Password
        {
            get { return _password; }
            set
            {
                new PasswordThrow().Validate(value);
                _password = value;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public string Email
        {
            get { return _email; }
            set
            {
                new EmailThrow().Validate(value);
                _email = value;
            }
        }

        [JsonProperty(Required = Required.Always)]
        public string GivenName
        {
            get { return _givenName; }
            set
            {
                new NameThrow().Validate(value);

                //capital letter
                _givenName = char.ToUpper(value[0]) + value.Substring(1);
            }
        }

        [JsonProperty(Required = Required.Always)]
        public string LastName
        {
            get { return _lastName; }
            set
            {
                new NameThrow().Validate(value);
                _lastName = char.ToUpper(value[0]) + value.Substring(1);
            }
        }

    }
}

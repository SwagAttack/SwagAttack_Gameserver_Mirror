
using System;
using DBInterface.DAL;
using DocumentDB.Repository;
using Models.User;

namespace Testing_to_Domain
{
    class Program
    {

        static void Main(string[] args)
        {
            User newUser = new User();
            newUser.Email = "ab@ab.dk";
            newUser.GivenName = "TestingName";
            newUser.LastName = "TestingLastName";
            newUser.Password = "123%%%aaaa";
            newUser.Username = "1337User";

            UserRepository<User> x = new UserRepository<User>();

            //x.AddUser(newUser);

            User xy = x.GetUserById("ab@ab.dk").Result;
            Console.WriteLine(xy.Email + xy.GivenName);
        }
    }
}

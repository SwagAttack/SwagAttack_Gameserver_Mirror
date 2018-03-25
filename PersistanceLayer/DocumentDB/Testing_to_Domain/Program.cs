
using System;
using System.Net;
using DBInterface.DAL;
using DBInterface.UnitOfWork;
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

            UnitOfWork xy = new UnitOfWork();

            xy.AddUser(newUser);
            Console.WriteLine(xy.GetUserById("ab@ab.dk").GivenName);

        }
    }
}

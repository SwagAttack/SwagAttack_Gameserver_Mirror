
using System;
using System.Net;
using DBInterface;
using DBInterface.UnitOfWork;
using DocumentDB.Repository;
using Models.User;

namespace Testing_to_Domain
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var newUser = new User
            {
                Email = "ab@ab.dk",
                GivenName = "TestingName",
                LastName = "TestingLastName",
                Password = "123%%%aaaa",
                Username = "1337User"
            };

            UnitOfWork xy = new UnitOfWork(new DbContext());

            xy._userRepository.AddUser(newUser).Wait();
            Console.WriteLine(xy._userRepository.GetUserByEmail("ab@ab.dk").Email);

            xy._userRepository.DeleteUserByEmail("ab@ab.dk");

            newUser.GivenName = "replacedName";
            xy._userRepository.ReplaceUser(newUser);

        }
    }
}

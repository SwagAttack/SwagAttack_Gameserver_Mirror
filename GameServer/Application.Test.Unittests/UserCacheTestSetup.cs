using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    internal static class FakeUserGenerator
    {
        private static string GenerateRandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        private static string Pass = "ADSAD";

        public static Dictionary<string, string> GenerateFakeUsers(int count)
        {
            var users = new Dictionary<string, string>();

            for (var i = 0; i < count; i++)
            {
                users[GenerateRandomString()] = Pass;
            }

            return users;
        }
    }
    [SetUpFixture]
    public class UserCacheTestSetup
    {
        public static Dictionary<string, string> FakeUsers { get; set; }
        [OneTimeSetUp]
        public void BeforeAnyTest()
        {
            FakeUsers = FakeUserGenerator.GenerateFakeUsers(50000);
        }
    }
}
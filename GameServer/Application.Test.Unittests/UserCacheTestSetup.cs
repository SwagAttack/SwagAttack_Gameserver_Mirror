using System;
using System.Collections.Generic;
using System.Linq;
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
            var dictionary = new Dictionary<string, string>();
            while (dictionary.Count != count)
            {
                dictionary.TryAdd(GenerateRandomString(), Pass);
            }

            return dictionary;

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
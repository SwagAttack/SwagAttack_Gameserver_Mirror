using Domain.Interfaces;
using Domain.Models;
using NUnit.Framework;

namespace Application.Test.Unittests
{

        [TestFixture]
        public class TestRepoTests
        {
            private Persistance.Repository.TestRepo uut;

            [SetUp]
            public void setup()
            {
                uut = new Persistance.Repository.TestRepo(new Persistance.Setup.DbContext(), "TestRepo");
            }

            [Test]
            public void TestCreate()
            {


                var user = new User()
                {
                    Email = "Swaggerdagger12@gmail.com",
                    GivenName = "Jonas",
                    LastName = "Andersen",
                    Password = "Random123",
                    Username = "Myusername123"
                };

                var test = uut.CreateItemAsync(user).Result;
                Assert.That(test == null);
            }

            [Test]
            public void TestShit()
            {

                var items = uut.GetItemsAsync(u => u.Username == "Myusername123").Result;
                Assert.IsNotEmpty(items);
            }

        }
    }
    
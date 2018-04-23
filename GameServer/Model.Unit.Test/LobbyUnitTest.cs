using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;
using Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace Model.Unit.Test
{
    [TestFixture]
    public class LobbyUnitTest
    {
        private Lobby _uut;
        private IUser _adminUser;


        [SetUp]
        public void setup()
        {
            _adminUser = Substitute.For<IUser>();
            _adminUser.Username = "MacroMan";
            _uut = new Lobby(_adminUser);
        }

        [Test]
        public void AddUser_Adds1FakeUsers_CollectionHas2Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            //act
            _uut.AddUser(fakeUser1);
            var result = _uut.Usernames;

            //assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddUser_Adds2FakeUsers_CollectionHas3Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            var result = _uut.Usernames;
            


            //assert
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddUser_Adds5FakeUser_CollectionHas6Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            IUser fakeUser3 = Substitute.For<IUser>();
            fakeUser3.Username = "IAmATestUser3";

            IUser fakeUser4 = Substitute.For<IUser>();
            fakeUser4.Username = "IAmATestUser4";

            IUser fakeUser5 = Substitute.For<IUser>();
            fakeUser5.Username = "IAmATestUser5";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            _uut.AddUser(fakeUser3);
            _uut.AddUser(fakeUser4);
            _uut.AddUser(fakeUser5);
            var result = _uut.Usernames;

            //assert
            Assert.That(result.Count, Is.EqualTo(6));
        }



        [Test]
        public void AddUser_Adds1FakeUser_CollectionHas1CorrectUsername()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            //act
            _uut.AddUser(fakeUser1);
            var result = _uut.Usernames.ElementAt(1);
            //assert
            Assert.That(result, Is.EqualTo(fakeUser1.Username));
        }
        

        [Test]
        public void RemoveUser_Adds2FakeUsers_Remove1FakeUser_CollectionHas2Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            _uut.RemoveUser(fakeUser1);
            var result = _uut.Usernames;



            //assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void RemoveUser_Adds5FakeUser_Remove4FakeUser_CollectionHas2Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            IUser fakeUser3 = Substitute.For<IUser>();
            fakeUser3.Username = "IAmATestUser3";

            IUser fakeUser4 = Substitute.For<IUser>();
            fakeUser4.Username = "IAmATestUser4";

            IUser fakeUser5 = Substitute.For<IUser>();
            fakeUser5.Username = "IAmATestUser5";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            _uut.AddUser(fakeUser3);
            _uut.AddUser(fakeUser4);
            _uut.AddUser(fakeUser5);
            _uut.RemoveUser(fakeUser5);
            _uut.RemoveUser(fakeUser4);
            _uut.RemoveUser(fakeUser3);
            _uut.RemoveUser(fakeUser2);
            var result = _uut.Usernames;

            //assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void UpdateAdmin_Adds2FakeUsers_UpdateAdmin_CollectionHas2Users()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            _uut.UpdateAdmin();
            var result = _uut.Usernames;



            //assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void UpdateAdmin_Adds2FakeUsers_UpdateAdmin_AdminUsernameIsCorrect()
        {
            //arrange
            IUser fakeUser1 = Substitute.For<IUser>();
            fakeUser1.Username = "IAmATestUser1";

            IUser fakeUser2 = Substitute.For<IUser>();
            fakeUser2.Username = "IAmATestUser2";

            //act
            _uut.AddUser(fakeUser1);
            _uut.AddUser(fakeUser2);
            _uut.UpdateAdmin();
            var result = _uut.AdminUserName;



            //assert
            Assert.That(result, Is.EqualTo(fakeUser1.Username));
        }

    }

}

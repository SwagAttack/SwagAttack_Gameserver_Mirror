using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Interfaces;

using NUnit.Framework;

namespace Model.Unit.Test.User
{
    
    [TestFixture]
    class ValidateUsername
    {
        private IUser uut;

        [SetUp]
        public void setup()
        {
            uut = new Models.User.User();
        }

        [Test]
        public void Username0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.Username == null);
        }

        [Test]
        public void Username7Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "Testtes", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 8 characters"));
        }
        [Test]
        public void Username8Characters_ThrowsNothing()
        {
            string test = "Testtest";
            Assert.That(() => uut.Username = test , Is.EqualTo(test));
        }

        [Test]
        public void Username19Characters_OK()
        {
            string test = "testtesttesttesttes";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void Username20Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "Testtes", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 8 characters"));
        }

        [Test]
        public void UsernameContainsNumbers_OK()
        {
            string test = "l33tt35t";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void UsernameWithSpecialLetter1_ThrowsArgumentException()
        {
            string test = "AuserWith!";
            Assert.That(() => uut.Username = test, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithSpecialLetter2_ThrowsArgumentException()
        {
            string test = "AuserWith%";
            Assert.That(() => uut.Username = test, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithSpecialLetter3_ThrowsArgumentException()
        {
            string test = "AuserWith)";
            Assert.That(() => uut.Username = test, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameContainsSmallAndLargeLetters_OK()
        {
            string test = "LeEtUsEr";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void UsernameWithWhiteSpaces_ThrowsArgumentException()
        {
            string test = "LeEt UsEr";
            Assert.That(() => uut.Username = test, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithOnlyWhiteSpaces_ThrowsArgumentException()
        {
            string test = "            ";
            Assert.That(() => uut.Username = test, Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value only be letters from a to z or numbers"));
        }
    }
}

using System;
using Domain.Interfaces;

using NUnit.Framework;

namespace Model.Unit.Test.User
{
    [TestFixture]
    internal class ValidateUsername
    {
        private IUser uut;
        [SetUp]
        public void setup()
        {
            uut = new Domain.Models.User();
        }

        [Test]
        public void Username0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.Username == null);
        }

        [Test]
        public void Username19Characters_OK()
        {
            var test = "testtesttesttesttes";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void Username20Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "Testtes",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 8 characters"));
        }

        [Test]
        public void Username7Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "Testtes",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 8 characters"));
        }

        [Test]
        public void Username8Characters_ThrowsNothing()
        {
            var test = "Testtest";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void UsernameContainsNumbers_OK()
        {
            var test = "l33tt35t";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void UsernameContainsSmallAndLargeLetters_OK()
        {
            var test = "LeEtUsEr";
            Assert.That(() => uut.Username = test, Is.EqualTo(test));
        }

        [Test]
        public void UsernameWithOnlyWhiteSpaces_ThrowsArgumentException()
        {
            var test = "            ";
            Assert.That(() => uut.Username = test,
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithSpecialLetter1_ThrowsArgumentException()
        {
            var test = "AuserWith!";
            Assert.That(() => uut.Username = test,
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithSpecialLetter2_ThrowsArgumentException()
        {
            var test = "AuserWith%";
            Assert.That(() => uut.Username = test,
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithSpecialLetter3_ThrowsArgumentException()
        {
            var test = "AuserWith)";
            Assert.That(() => uut.Username = test,
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("value only be letters from a to z or numbers"));
        }

        [Test]
        public void UsernameWithWhiteSpaces_ThrowsArgumentException()
        {
            var test = "LeEt UsEr";
            Assert.That(() => uut.Username = test,
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("value only be letters from a to z or numbers"));
        }
    }
}
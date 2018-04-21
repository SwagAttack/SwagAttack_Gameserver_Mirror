
using System;
using Domain.Interfaces;
using NUnit.Framework;

namespace Model.Unit.Test.User
{
    [TestFixture]
    class ValidatePassword
    {
        private IUser uut;
        [SetUp]
        public void setup()
        {
            uut = new Domain.Models.User();
        }

        [Test]
        public void Password0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Password = "",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.Password == null);
        }

        [Test]
        public void Password19Characters_OK()
        {
            var test = "testtesttesttesttes";
            Assert.That(() => uut.Password = test, Is.EqualTo(test));
        }

        [Test]
        public void Password20Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Password = "Testtes",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 8 characters"));
        }

        [Test]
        public void PasswordManyCharacters_OK()
        {
            var test = "testtesttesttesttessadafgfwegfwegwegwefewfwefwefewfewfwefwfwfwfwefwefwfwfewfwefewfe";
            Assert.That(() => uut.Password = test, Is.EqualTo(test));
        }

        [Test]
        public void PasswordWithNumbers_OK()
        {
            var test = "testingIfNumbersIsOK123";
            Assert.That(() => uut.Password = test, Is.EqualTo(test));
        }

        [Test]
        public void PasswordWithSpecial_OK()
        {
            var test = "testingIfSpecialsIsOK!¤()æøå#";
            Assert.That(() => uut.Password = test, Is.EqualTo(test));
        }
    }
}

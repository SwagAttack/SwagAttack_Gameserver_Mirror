using System;
using Domain.Interfaces;
using NUnit.Framework;

namespace Domain.Test.Unittests.User
{
    class ValidateLastName
    {

        private IUser uut;

        [SetUp]
        public void setup()
        {
            uut = new Domain.Models.User();
        }

        [Test]
        public void Email0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.LastName = "",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.LastName == null);
        }

        [Test]
        public void LastName1Character_ThrowsArgumentException()
        {
            Assert.That(() => uut.LastName = "A",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Initials is not allowed"));
            Assert.That(() => uut.LastName == null);
        }

        [Test]
        public void LastNameInitial_ThrowsArgumentException()
        {
            Assert.That(() => uut.LastName = "A.",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Initials is not allowed"));
            Assert.That(() => uut.LastName == null);
        }

        [Test]
        public void LastName2Character_ThrowsNothing()
        {
            Assert.That(() => uut.LastName = "Bo",
                Throws.Nothing);
            Assert.That(() => uut.LastName == "Bo");
        }

        [Test]
        public void LastNameSmall_ThrowsNothing()
        {
            Assert.That(() => uut.LastName = "bo",
                Throws.Nothing);
            Assert.That(() => uut.LastName == "Bo");
        }

        [Test]
        public void LastNameWithNumbers_ThrowsArgumentException()
        {
            Assert.That(() => uut.LastName = "1bo", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Name can only contain letters"));

        }

        [Test]
        public void LastNameWithSpecial_ThrowsArgumentException()
        {
            Assert.That(() => uut.LastName = "%bo", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Name can only contain letters"));

        }
    }
}

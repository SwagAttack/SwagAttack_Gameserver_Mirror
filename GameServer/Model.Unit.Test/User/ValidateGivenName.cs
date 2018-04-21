using System;
using Domain.Interfaces;
using NUnit.Framework;

namespace Model.Unit.Test.User
{
    [TestFixture]
    class ValidateGivenName
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
            Assert.That(() => uut.GivenName = "",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.GivenName == null);
        }

        [Test]
        public void GivenName1Character_ThrowsArgumentException()
        {
            Assert.That(() => uut.GivenName = "A",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Initials is not allowed"));
            Assert.That(() => uut.GivenName == null);
        }

        [Test]
        public void GivenNameInitial_ThrowsArgumentException()
        {
            Assert.That(() => uut.GivenName = "A.",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Initials is not allowed"));
            Assert.That(() => uut.GivenName == null);
        }

        [Test]
        public void GivenName2Character_ThrowsNothing()
        {
            Assert.That(() => uut.GivenName = "Bo",
                Throws.Nothing);
            Assert.That(() => uut.GivenName == "Bo");
        }

        [Test]
        public void GivenNameSmall_ThrowsNothing()
        {
            Assert.That(() => uut.GivenName = "bo",
                Throws.Nothing);
            Assert.That(() => uut.GivenName == "Bo");
        }

        [Test]
        public void GivenNameWithNumbers_ThrowsArgumentException()
        {
            Assert.That(() => uut.GivenName = "1bo", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Name can only contain letters"));

        }

        [Test]
        public void GivenNameWithSpecial_ThrowsArgumentException()
        {
            Assert.That(() => uut.GivenName = "%bo", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Name can only contain letters"));

        }

    }
}

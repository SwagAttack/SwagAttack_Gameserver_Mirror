using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Interfaces;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Model.Unit.Test.User
{   
    [TestFixture]
    class ValidateEmailcs
    {
        private IUser uut;

        [SetUp]
        public void setup()
        {
            uut = new Models.User.User();
        }

        [Test]
        public void Email0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Email = "",
                Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be empty!"));
            Assert.That(() => uut.Password == null);
        }

        [Test]
        public void Email4CharactersLong_ThrowsArgumentException()
        {
            Assert.That(() => uut.Email = "a@b.", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("value cannot be less than 5 characters"));
        }

        [Test]
        public void EmailDoesNotContainAt_ThrowsArgumentException()
        {
            Assert.That(() => uut.Email = "ab.dk.", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Not a valid Email"));
        }

        [Test]
        public void EmailDoesNotContainDot_ThrowsArgumentException()
        {
            Assert.That(() => uut.Email = "ab@dk", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Not a valid Email"));
        }

        [Test]
        public void Email5CharactersLong_ThrowsNothing()
        {
            Assert.That(() => uut.Email = "a@b.d", Throws.Nothing);
        }

        [Test]
        public void EmailContainsAtAndDot_throwsNothing()
        {
            Assert.That(() => uut.Email = "ab@a.dk",
                Throws.Nothing);
        }
    }
}

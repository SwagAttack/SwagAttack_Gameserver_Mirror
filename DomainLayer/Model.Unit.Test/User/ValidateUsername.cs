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
        public void Username0Characters_isNotSet()
        {
            uut.Username = "";
            Assert.That(() => uut.Username == null);
        }

        [Test]
        public void Username0Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Username cannot be empty!"));
            Assert.That(() => uut.Username == null);
        }

        [Test]
        public void Username7Characters_ThrowsArgumentException()
        {
            Assert.That(() => uut.Username = "Testtes", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Username cannot be less than 8 characters"));
        }
        [Test]
        public void Username8Characters_ThrowsNothing()
        {
            Assert.That(() => uut.Username = "Testtest" , Throws.Nothing);
        }

    }
}

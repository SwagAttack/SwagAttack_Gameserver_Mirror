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
        public void Username0Characters_returnsFalse()
        {
            Assert.That(() => uut.Username = "", Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Name cannot be empty!"));
        }

        [Test]
        public void Username7Characters_returnsFalse()
        {

        }
        [Test]
        public void Username8Characters_returnsTrue()
        {

        }

    }
}

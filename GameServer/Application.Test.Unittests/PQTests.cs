using System;
using System.Threading;
using Application.Misc;
using Medallion.Collections;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class PQTests
    {
        private PriorityQueue<ExpirationMark> uut;

        [SetUp]
        public void Setup()
        {
            uut = new PriorityQueue<ExpirationMark>(new ExpirationComparer());
        }

        //[Test]
        //public void ads()
        //{
        //    uut.Add(ExpirationMark.GetNewMark("Jonas"));
        //    Thread.Sleep(500);
        //    uut.Add(ExpirationMark.GetNewMark("Andersen"));
        //    uut.Add(new ExpirationMark("Swag", DateTime.Now));
        //    uut.Remove(ExpirationMark.GetNewMark("Andersen"));

        //    Assert.That(null);
        //}
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Misc;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class SmartLockUnitTests
    {
        SmartLock _uut = new SmartLock();

        private Thread _testThreadOne;
        private Thread _testThreadTwo;
        private Thread _testThreadThree;

        private int _testCounter;

        [SetUp]
        public void SetUp()
        {
            _testCounter = 0;
        }
        
        [Test]
        public void LockAsync_ThreadsIncrementTheSameVariable_CountIsCorrect()
        {
            // Arrange

            void CounterFunc()
            {
                // Sleeps before running the loop
                Thread.Sleep(500);

                for (int i = 0; i < 10000; i++)
                {
                    using (_uut.LockAsync().Result)
                    {
                        _testCounter++;
                    }
                }
            }

            _testThreadOne = new Thread(CounterFunc);
            _testThreadTwo = new Thread(CounterFunc);
            _testThreadThree = new Thread(CounterFunc);

            // Act

            _testThreadOne.Start();
            _testThreadTwo.Start();
            _testThreadThree.Start();

            _testThreadOne.Join();
            _testThreadTwo.Join();
            _testThreadThree.Join();

            // Assert

            Assert.That(_testCounter, Is.EqualTo(30000));
        }

        [Test]
        public void Lock_ThreadsIncrementTheSameVariable_CountIsCorrect()
        {
            // Arrange

            void CounterFunc()
            {
                // Sleeps before running the loop
                Thread.Sleep(500);

                for (int i = 0; i < 10000; i++)
                {
                    using (_uut.Lock())
                    {
                        _testCounter++;
                    }
                }
            }

            _testThreadOne = new Thread(CounterFunc);
            _testThreadTwo = new Thread(CounterFunc);
            _testThreadThree = new Thread(CounterFunc);

            // Act

            _testThreadOne.Start();
            _testThreadTwo.Start();
            _testThreadThree.Start();

            _testThreadOne.Join();
            _testThreadTwo.Join();
            _testThreadThree.Join();

            // Assert

            Assert.That(_testCounter, Is.EqualTo(30000));
        }

    }
}
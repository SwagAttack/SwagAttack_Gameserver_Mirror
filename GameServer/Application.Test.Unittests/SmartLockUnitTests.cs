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
        
        [TestCase(10000, 30000)]
        [TestCase(100000, 300000)]
        [TestCase(1000000, 3000000)]
        public void LockAsync_ThreeTasksIncrementTheSameVariable_CountIsCorrect(int countTo, int result)
        {
            // Arrange

            async Task CounterFunc()
            {
                // Sleeps before running the loop
                Thread.Sleep(500);

                for (int i = 0; i < countTo; i++)
                {
                    using(await _uut.LockAsync())
                    {
                        _testCounter++;
                    }
                }
            }

            // Act

            var first = CounterFunc();
            var second = CounterFunc();
            var third = CounterFunc();

            Task.WaitAll(first, second, third);

            // Assert

            Assert.That(_testCounter, Is.EqualTo(result));
        }

        [TestCase(10000, 30000)]
        [TestCase(100000, 300000)]
        [TestCase(1000000, 3000000)]
        public void Lock_ThreeThreadsIncrementTheSameVariable_CountIsCorrect(int countTo, int result)
        {
            // Arrange

            void CounterFunc()
            {
                // Sleeps before running the loop
                Thread.Sleep(500);

                for (int i = 0; i < countTo; i++)
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

            Assert.That(_testCounter, Is.EqualTo(result));
        }

    }
}
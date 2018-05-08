using System;
using System.Collections.Generic;
using System.Linq;
using Application.Misc;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class CacheListUnitTests
    {
        private CacheList<string> _uut;

        [SetUp]
        public void SetUp()
        {
            _uut = new CacheList<string>();
        }

        [Test]
        public void Add_AddThreeStringsNotInOrder_UutOrderIsCorrect()
        {
            _uut.Add("Third", DateTime.Now.AddMinutes(4));
            _uut.Add("Second", DateTime.Now.AddMinutes(3));
            _uut.Add("First", DateTime.Now.AddMinutes(2));

            Assert.That(_uut.Collection[0].Item == "First");
            Assert.That(_uut.Collection[1].Item == "Second");
            Assert.That(_uut.Collection[2].Item == "Third");
        }

        [Test]
        public void Add_AddTenRandomStringsWithRandomDateTimes_UutOrderIsCorrect()
        {
            List<string> fakeStrings = FakeUserGenerator.GenerateFakeUsers(10).Keys.ToList();
            var random = new Random();
            foreach (var fakeString in fakeStrings)
            {
                _uut.Add(fakeString, DateTime.Now.AddMinutes(random.Next(20)));
            }

            for (int i = _uut.Collection.Count - 1; i > 0; i--)
            {
                Assert.That(_uut.Collection[i].Expiration.CompareTo(_uut.Collection[i - 1].Expiration), Is.EqualTo(1)); // Assert that the current item is after the one standing before it
            }
        }

        [Test]
        public void Find_ItemExists_ReturnsIndexOfItem()
        {
            List<string> fakeStrings = FakeUserGenerator.GenerateFakeUsers(10).Keys.ToList();

            var random = new Random();
            foreach (var fakeString in fakeStrings)
            {
                _uut.Add(fakeString, DateTime.Now.AddMinutes(random.Next(20)));
            }

            // Act
            var index = _uut.Find(fakeStrings[0]);

            // Assert
            Assert.That(_uut.Collection[index].Item == fakeStrings[0]);
        }

        [Test]
        public void Find_ItemDoesNotExist_ReturnsNegative()
        {
            var index = _uut.Find("BadItem");
            Assert.That(index < 0);
        }

        [Test]
        public void Remove_ItemExists_RemovesItem()
        {
            List<string> fakeStrings = FakeUserGenerator.GenerateFakeUsers(10).Keys.ToList();

            var random = new Random();
            foreach (var fakeString in fakeStrings)
            {
                _uut.Add(fakeString, DateTime.Now.AddMinutes(random.Next(20)));
            }

            // Act
            var index = _uut.Find(fakeStrings[0]);
            _uut.Remove(index);

            // Assert
            Assert.That(_uut.Collection.Count(t => t.Item == fakeStrings[0]) == 0);
        }

        [Test]
        public void Update_AddItemsThenUpdateSomeItems_ReturnsTrueAndUutOrderIsCorrect()
        {
            List<string> fakeStrings = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                fakeStrings.Add(FakeUserGenerator.GenerateRandomString());
            }

            var random = new Random();
            foreach (var fakeString in fakeStrings)
            {
                _uut.Add(fakeString, DateTime.Now.AddMinutes(random.Next(20)));
            }

            // Act

            var index = _uut.Find(fakeStrings[5]);
            var firstResult = _uut.Update(index, DateTime.Now.AddMinutes(random.Next(5)));

            index = _uut.Find(fakeStrings[3]);
            var secondResult = _uut.Update(index, DateTime.Now.AddMinutes(random.Next(10)));

            // Assert
            Assert.That(firstResult && secondResult);
            for (int i = _uut.Collection.Count - 1; i > 0; i--)
            {
                Assert.That(_uut.Collection[i].Expiration.CompareTo(_uut.Collection[i - 1].Expiration), Is.EqualTo(1)); // Assert that the current item is after the one standing before it
            }
        }

        [Test]
        public void Update_DoesntContainItem_ReturnsFalse()
        {
            var result = _uut.Update(4, DateTime.Now);
            Assert.That(!result);
        }

        [Test]
        public void ContainsOutDatedItem_ContainsAnItemThatIsOutdated_ReturnsTrue()
        {
            _uut.Add("OutdatedItem", DateTime.Now.AddMinutes(-1));
            var result = _uut.ContainsOutdatedItem(DateTime.Now);
            Assert.That(result);
        }

        [Test]
        public void ContainsOutDatedItem_ContainsItemsButNoneAreOutdated_ReturnsFalse()
        {
            _uut.Add("NotOutDated", DateTime.Now.AddMinutes(1));
            var result = _uut.ContainsOutdatedItem(DateTime.Now);
            Assert.That(!result);
        }

        [Test]
        public void ContainsOutDatedItem_ContainsNoItems_ReturnsFalse()
        {
            var result = _uut.ContainsOutdatedItem(DateTime.Now);
            Assert.That(!result);
        }

        [Test]
        public void RemoveAndGet_ContainsItem_ReturnsItemAndRemovesIt()
        {
            _uut.Add("Item", DateTime.Now.AddMinutes(1));
            var item = _uut.RemoveAndGet();
            Assert.That(item == "Item");
            Assert.That(_uut.Collection.Count == 0);
        }

        [Test]
        public void RemoveAndGet_ContainsNoItem_ThrowsIndexOutOfRangeException()
        {
            Assert.Throws(typeof(IndexOutOfRangeException),() => _uut.RemoveAndGet());
        }
    }
}
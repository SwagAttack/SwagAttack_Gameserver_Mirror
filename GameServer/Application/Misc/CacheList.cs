using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Misc
{
    public class CacheList<T> where T : IComparable<T>
    {
        private readonly List<ContainedItem> _collection;

        public IReadOnlyList<ContainedItem> Collection => _collection;

        public CacheList()
        {
            _collection = new List<ContainedItem>();
        }

        public CacheList(int capacity)
        {
            _collection = new List<ContainedItem>(capacity);
        }

        public void Add(T item, DateTime expiration)
        {
            var index = _collection.Count - 1;
            while (index >= 0)
            {
                if (_collection[index].Expiration.IsBefore(expiration))
                {
                    _collection.Insert(index + 1,
                        new ContainedItem { Expiration = expiration, Item = item});
                    return;
                }
                index--;
            }
            _collection.Insert(0, 
                new ContainedItem { Expiration = expiration, Item = item});
        }

        public int Find(T item)
        {
            return _collection.FindIndex(c => c.Item.CompareTo(item) == 0);
        }

        public void Remove(int index)
        {
            _collection.RemoveAt(index);
        }

        public bool Update(int index, DateTime expiration)
        {
            try
            {
                var item = _collection[index];
                _collection.RemoveAt(index);
                Add(item.Item, expiration);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool ContainsOutdatedItem(DateTime compareTo)
        {
            if (_collection.Count == 0) return false;
            return _collection[0].Expiration.IsBefore(compareTo);

        }

        public T RemoveAndGet()
        {
            if(_collection.Count == 0)
                throw new IndexOutOfRangeException();
            var item = _collection[0];
            _collection.RemoveAt(0);
            return item.Item;
        }

        public class ContainedItem
        {
            public T Item { get; set; }
            public DateTime Expiration { get; set; }
        }
    }

    internal static class DateTimeExtentions
    {
        /// <summary>
        /// Returns whether current is before target
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsBefore(this DateTime current, DateTime target)
        {
            return current.CompareTo(target) < 0;
        }
    }
}
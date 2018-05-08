using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Misc
{
    public class CacheList<T>
    {
        private readonly LinkedList<ContainedItem> _collection;

        public CacheList()
        {
            _collection = new LinkedList<ContainedItem>();
        }

        public IReadOnlyList<ContainedItem> Collection => _collection.ToList();

        public void Add(T item, DateTime expiration, out ICacheHandle handle)
        {
            LinkedListNode<ContainedItem> node = null;

            var current = _collection.Last;
            while (current != null)
            {
                if (!expiration.IsBefore(current.Value.Expiration))
                    break;
                current = current.Previous;
            }

            if (current == null)
                node = _collection.AddFirst(ContainedItem.CreateItem(item, expiration));
            else
                node = _collection.AddAfter(current, ContainedItem.CreateItem(item, expiration));

            handle = CacheHandle.CreateHandle(node);
        }

        public void Remove(ICacheHandle handle)
        {
            _collection.Remove(CacheHandle.GetHandle(handle).NodeItem);
        }

        public void Update(ICacheHandle handle, DateTime expiration)
        {
            var updatingNote = CacheHandle.GetHandle(handle).NodeItem;
            _collection.Remove(updatingNote);
            Add(updatingNote.Value.Item, expiration, out var newHandle);
            CacheHandle.GetHandle(handle).Update(newHandle);
        }

        public bool ContainsOutdatedItem(DateTime compareTo)
        {
            if (_collection.Count == 0) return false;
            return _collection.First.Value.Expiration.IsBefore(compareTo);
        }

        public T RemoveAndGet()
        {
            if (_collection.Count == 0)
                throw new IndexOutOfRangeException();
            var item = _collection.First.Value;
            _collection.RemoveFirst();
            return item.Item;
        }

        public class ContainedItem
        {
            public T Item { get; set; }
            public DateTime Expiration { get; set; }

            public static ContainedItem CreateItem(T item, DateTime expiration)
            {
                return new ContainedItem {Expiration = expiration, Item = item};
            }
        }

        private class CacheHandle : ICacheHandle
        {
            private CacheHandle(LinkedListNode<ContainedItem> node)
            {
                NodeItem = node;
            }

            public LinkedListNode<ContainedItem> NodeItem { get; private set; }

            public void Update(ICacheHandle handle)
            {
                var converted = GetHandle(handle);
                NodeItem = converted.NodeItem;
            }

            public static ICacheHandle CreateHandle(LinkedListNode<ContainedItem> node)
            {
                return new CacheHandle(node);
            }

            public static CacheHandle GetHandle(ICacheHandle handle)
            {
                var target = handle as CacheHandle;
                return target;
            }
        }
    }

    public interface ICacheHandle
    {
    }

    internal static class DateTimeExtentions
    {
        /// <summary>
        ///     Returns whether current is before target
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
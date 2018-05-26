using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Misc
{
    public class CacheList<TClass> where TClass : class
    {
        private readonly LinkedList<ContainedItem> _collection;
        public IReadOnlyList<ContainedItem> Collection => _collection.ToList();

        public CacheList()
        {
            _collection = new LinkedList<ContainedItem>();
        }

        public void Add(TClass item, DateTime expiration, out ICacheHandle handle)
        {
            Add(item, expiration, out LinkedListNode<ContainedItem> node);
            handle = CreateHandle(node);
        }

        public void Remove(ICacheHandle cacheHandle)
        {
            var handle = GetHandle(cacheHandle);
            if (!ConfirmHandle(handle))
                return;
            _collection.Remove(handle.Node);
        }

        public void Update(ICacheHandle cacheHandle, DateTime expiration, TClass item = null)
        {
            var handle = GetHandle(cacheHandle);
            if (!ConfirmHandle(handle))
                return;

            _collection.Remove(handle.Node);

            Add(item ?? handle.Node.Value.Item, expiration, out LinkedListNode<ContainedItem> newNode);
            handle.Node = newNode;
        }

        public bool ContainsOutdatedItem(DateTime compareTo)
        {
            if (_collection.Count == 0) return false;
            return _collection.First.Value.Expiration.IsBefore(compareTo);
        }

        public TClass RemoveAndGet()
        {
            if (_collection.Count == 0)
                throw new IndexOutOfRangeException();
            var item = _collection.First.Value;
            _collection.RemoveFirst();
            return item.Item;
        }

        private void Add(TClass item, DateTime expiration, out LinkedListNode<ContainedItem> node)
        {
            var current = _collection.Last;
            while (current != null && expiration.IsBefore(current.Value.Expiration))
            {
                current = current.Previous;
            }

            current = current == null ?
                _collection.AddFirst(new ContainedItem(item, expiration)) :
                _collection.AddAfter(current, new ContainedItem(item, expiration));

            node = current;
        }

        #region CacheHandle

        private class CacheHandle : ICacheHandle
        {
            public CacheHandle(LinkedListNode<ContainedItem> node)
            {
                Node = node;
            }

            public LinkedListNode<ContainedItem> Node { get; set; }
        }

        private static ICacheHandle CreateHandle(LinkedListNode<ContainedItem> node)
        {
            return new CacheHandle(node);
        }

        private static CacheHandle GetHandle(ICacheHandle cacheHandle)
        {
            return cacheHandle is CacheHandle handle ? handle : null;
        }

        private bool ConfirmHandle(CacheHandle handle)
        {
            if (handle == null) return false;
            if (handle.Node.List != _collection) return false;
            return true;
        }

        #endregion

        public class ContainedItem
        {
            public TClass Item { get; internal set; }
            public DateTime Expiration { get; internal set; }

            internal ContainedItem(TClass item, DateTime expiration)
            {
                Item = item;
                Expiration = expiration;
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
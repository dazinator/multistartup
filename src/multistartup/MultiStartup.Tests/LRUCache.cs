namespace MultiStartup.Tests
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class LRUCache<TKey, TValue>
    {
        private int _capacity;
        private Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> _cacheMap = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>();
        private LinkedList<LRUCacheItem<TKey, TValue>> _lruList = new LinkedList<LRUCacheItem<TKey, TValue>>();

        public LRUCache(int capacity)
        {
            _capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public TValue Get(TKey key)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                var value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddLast(node);
                return value;
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(TKey key, TValue val)
        {
            if (_cacheMap.Count >= _capacity)
            {
                RemoveFirst();
            }

            var cacheItem = new LRUCacheItem<TKey, TValue>(key, val);
            var node = new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);
            _lruList.AddLast(node);
            _cacheMap.Add(key, node);
        }

        private void RemoveFirst()
        {
            // Remove from LRUPriority
            var node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node.Value.Key);
        }
    }
}

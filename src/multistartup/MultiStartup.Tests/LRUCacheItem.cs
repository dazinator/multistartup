namespace MultiStartup.Tests
{
    internal class LRUCacheItem<TKey, TValue>
    {
        public LRUCacheItem(TKey k, TValue v)
        {
            Key = k;
            Value = v;
        }

        internal TKey Key;
        internal TValue Value;
    }
}

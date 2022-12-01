namespace InvoiceServer.Common.Cache
{
    /// <summary>
    /// The generic base class of all cache response classes
    /// </summary>
    /// <typeparam name="T">Type of cache key</typeparam>
    /// <typeparam name="U">Type of cache data</typeparam>
    public abstract class BaseResponseCache<T, U>
    {
        #region Fields, Properties
        private readonly IMemoryCache<U> _cache;
        protected IMemoryCache<U> InnerCache
        {
            get { return this._cache; }
        }

        protected string CacheRegionName { get; }

        #endregion

        #region Constructor, Destructor
        protected BaseResponseCache(string regionName)
        {
            this.CacheRegionName = regionName;
            this._cache = MemoryCacheFactory.Instance.GetCache<U>(this.CacheRegionName);
        }

        ~BaseResponseCache()
        {
            this._cache.Clear();
        }
        #endregion

        #region Overrideable methods
        protected virtual string GetCacheKey(T cacheId)
        {
            return cacheId.ToString();
        }

        public virtual bool Contains(T cacheId)
        {
            string key = GetCacheKey(cacheId);
            return this.InnerCache.Contains(key);
        }

        public virtual U GetResponse(T cacheId)
        {
            string key = GetCacheKey(cacheId);
            if (this.InnerCache.Contains(key))
            {
                return this.InnerCache.Get(key);
            }
            return default(U);
        }

        public virtual void AddResponse(T cacheId, U cacheData)
        {
            if (cacheData != null)
            {
                string key = GetCacheKey(cacheId);
                this.InnerCache.Set(key, cacheData);
            }
        }

        public virtual void RemoveResponse(T cacheId)
        {
            string key = GetCacheKey(cacheId);
            if (this.InnerCache.Contains(key))
            {
                this.InnerCache.Remove(key);
            }
        }

        public void RemoveIfKeyStartsWith(T cacheId)
        {
            string key = GetCacheKey(cacheId);
            this._cache.RemoveIfKeyStartsWith(key);
        }

        public virtual void Clear()
        {
            this._cache.Clear();
        }
        #endregion
    }
}
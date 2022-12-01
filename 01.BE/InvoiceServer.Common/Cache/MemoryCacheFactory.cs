using System;
using System.Collections.Generic;

namespace InvoiceServer.Common.Cache
{
    public class MemoryCacheFactory
    {
        private const int DefaultCacheItemTimeout = 120; // in minutes

        #region Fields, Properties
        private static readonly object _lockObject = new object();
        private readonly Dictionary<string, object> _cacheList = new Dictionary<string, object>();
        private TimeSpan _cacheItemTimeout = TimeSpan.FromMinutes(DefaultCacheItemTimeout);
        private bool _cacheItemEnabled = true;
        private bool _intialized = false;

        private static readonly MemoryCacheFactory _instance = new MemoryCacheFactory();
        public static MemoryCacheFactory Instance
        {
            get { return _instance; }
        }
        #endregion

        #region Constructor, Methods
        private MemoryCacheFactory()
        {
        }

        public void Initialize(bool enabled, int timeout)
        {
            Ensure.Argument.Is(timeout > 0);
            Ensure.Not<InvalidOperationException>(this._intialized, "This method has been called before.");

            this._intialized = true;
            this._cacheItemEnabled = enabled;
            this._cacheItemTimeout = TimeSpan.FromMinutes(timeout);
        }

        public IMemoryCache<T> GetCache<T>(string cacheName)
        {
            Ensure.Argument.ArgumentNotNullOrEmpty(cacheName);

            lock (_lockObject)
            {
                if (!this._cacheList.ContainsKey(cacheName))
                {
                    var cacheT = new MemoryCache<T>(cacheName, this._cacheItemTimeout, this._cacheItemEnabled);
                    this._cacheList.Add(cacheName, cacheT);
                }

                return this._cacheList[cacheName] as IMemoryCache<T>;
            }
        }
        #endregion
    }
}

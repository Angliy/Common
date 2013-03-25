using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Common.Caching
{
    /// <summary>
    /// Cache settings for the Cache instance.
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Used to prefix all the cache keys.
        /// </summary>
        public string PrefixForCacheKeys = "Common";
        
        
        /// <summary>
        /// Indicates if using prefixes.
        /// </summary>
        public bool UsePrefix = true;


        /// <summary>
        /// Default cache item priority.
        /// </summary>
        public CacheItemPriority DefaultCachePriority = CacheItemPriority.Normal;


        /// <summary>
        /// Default flag indicating if sliding expiration is enabled.
        /// </summary>
        public bool DefaultSlidingExpirationEnabled = false;


        /// <summary>
        /// Default amount of time to keep item in cache.
        /// 10 mins.
        /// </summary>
        public int DefaultTimeToLive = 600;
    }
}

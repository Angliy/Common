using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;


namespace Common.Caching
{

    /// <summary>
    /// Helper utility to "reflect"( describe ) the items in the cache 
    /// and manage the removal of cache items.
    /// </summary>
    public class CacheManager
    {
        private ICache _cache;


        /// <summary>
        /// Create using initialization.
        /// </summary>
        public CacheManager()
        {
            _cache = new CacheAspNet();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache"></param>
        public CacheManager(ICache cache)
        {
            _cache = cache;
        }


        /// <summary>
        /// Underlying cache
        /// </summary>
        public ICache Cache
        {
            get { return _cache; }
        }


        /// <summary>
        /// Get the items in the cache and their types.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public IList<CacheItemDescriptor> GetDescriptors()
        {
            IList<CacheItemDescriptor> descriptorList = new List<CacheItemDescriptor>();                
            ICollection keys = _cache.Keys;
            IEnumerator enumerator = keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                string key = enumerator.Current as string;
                object cacheItem = _cache.Get(key);
                descriptorList.Add(new CacheItemDescriptor(key, cacheItem.GetType().FullName));
            }

            // Sort the cache items by their name
            ((List<CacheItemDescriptor>)descriptorList).Sort(
              delegate(CacheItemDescriptor c1, CacheItemDescriptor c2)
              {
                  return c1.Key.CompareTo(c2.Key);
              });
            return descriptorList;
        }
    }
}

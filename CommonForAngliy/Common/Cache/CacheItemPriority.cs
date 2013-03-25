using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;



namespace Common.Caching
{

    /// <summary>
    /// Priority for cache items.
    /// </summary>
    public enum CacheItemPriority
    {
        /// <summary>
        /// Very Likely to be deleted.
        /// </summary>
        Low,

        /// <summary>
        /// Somewhat likely to be deleted.
        /// </summary>        
        Normal,

        /// <summary>
        /// Less likely to be deleted.
        /// </summary>
        High,

        /// <summary>
        /// Should not be deleted.
        NotRemovable,

        /// <summary>
        /// The default value for a cached item's priority is
        /// <see cref="Spring.Caching.CachePriority.Normal"/>.
        /// </summary>
        Default = Normal
    }
}

using System;
using System.Runtime.Caching;

namespace CourseApplicability.API.CacheManager
{
    public abstract class BaseCacheManager
    {
        #region "Properties"
        /// <summary>
        /// Provides the access of Cache Object.
        /// </summary>
        public static ObjectCache Cache { get { return MemoryCache.Default; } }

        #endregion

        #region "Abstract Members"
        /// <summary>
        /// Represents a property which will provide an acces of default cache policy.
        /// </summary>
        public abstract CacheItemPolicy DefaultCachePolicy { get; set; }
        /// <summary>
        /// This methods add an 'Object' in cache.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <param name="data">This is the data object which needs to store in Cache.</param>
        public abstract void Add<T>(string key, T data) where T : class;
        /// <summary>
        /// This methods add an 'Object' in cache with custom CachePolicy
        /// </summary>
        /// <typeparam name="T">Object Type and should be the a Class type</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <param name="data">This is the data which needs to store in Cache.</param>
        /// <param name="cachePolicy">This is the custom cache policy which will apply on the provided object while storing it in Cache.</param>
        public abstract void Add<T>(string key, T data, CacheItemPolicy cachePolicy) where T : class;

        /// <summary>
        /// This methods add an 'Object' in cache.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <param name="data">This is the data object which needs to store in Cache.</param>
        public abstract void Add<T>(string key, T data, DateTimeOffset absoluteExpiration) where T : class;

        /// <summary>
        /// This method provides an Object from Cache by uniquely identify it using the provided 'Key'.
        /// </summary>
        /// <typeparam name="T">Object Type and should be the a Class type.</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <returns>The reqired Object which is identified using the provided key.</returns>
        public abstract T Get<T>(string key) where T : class;
        /// <summary>
        /// This method is used to remove the Object from Cache.
        /// </summary>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        public abstract void Remove(string key);
        /// <summary>
        /// This method is checks whether the Object is available in the cache or not.
        /// </summary>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <returns>True if records exists in Cache which is identified using the provided key.</returns>
        public abstract Boolean IsAdded(string key);

        #endregion
    }
}

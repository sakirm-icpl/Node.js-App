using PollManagement.API.CacheManager;
using PollManagement.API.Helper;
using log4net;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace PollManagement.API.CacheManager
{
    public class CacheManager : BaseCacheManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CacheManager));

        #region "Data Members"
        /// <summary>
        /// It will holds the default value of CacheItemPolicy
        /// </summary>
        private static CacheItemPolicy _cachePolicy;
        /// <summary>
        /// It provides the default expiration time limit for Cache, it values should be consider as the number of minutes.
        /// </summary>
        private static readonly Int32 _defaultExpirationTime = 10;
        #endregion

        #region "Constructor"
        /// <summary>
        /// Static constructor, which will initalized the 'CacheItemPolicy' object. 
        /// </summary>
        static CacheManager()
        {
            //Initalized the default CacheItemPolicy object.
            _cachePolicy = new CacheItemPolicy();
            //_cachePolicy.AbsoluteExpiration
            _cachePolicy.SlidingExpiration = TimeSpan.FromMinutes(_defaultExpirationTime);
        }
        /// <summary>
        /// Initializes a new instance of the CacheManagement.CacheManager class.
        /// </summary>
        public CacheManager() { }

        #endregion

        #region "Override Methods"
        /// <summary>
        /// Represents a property which will provide an acces of default cache policy.
        /// Default CacheItempolicy uses 'AbsoluteExpiration'
        /// </summary>
        public override CacheItemPolicy DefaultCachePolicy
        {
            get { return _cachePolicy; }
            set { _cachePolicy = value; }
        }
        /// <summary>
        /// This methods is use to add an 'Object' in cache.
        /// </summary>
        /// <typeparam name="T">Object Type and it should be the a Class</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <param name="data">This is the data object which needs to store in Cache.</param>
        public override void Add<T>(string key, T data)
        {
            try
            {
                if (String.IsNullOrEmpty(key))
                {
                    throw new NullReferenceException("Key should not be blank.");
                }
                if (data == null)
                {
                    throw new NullReferenceException("Data should not be null.");
                }
                //if (IsAdded(key))
                //{
                //    throw new Exception("Key is already in use.");
                //}
                lock (Cache)
                {
                    try
                    {
                        if (Cache.Contains(key))
                            Cache.Remove(key);
                        Cache.Add(new CacheItem(key, data), DefaultCachePolicy);
                    }
                    catch (Exception)
                    { }
                }
            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in adding cache key {0} into Cache. :- {1}", key, Utilities.GetDetailedException(ex))); }
        }
        /// <summary>
        /// This methods is use to add an 'Object' in cache with custom CachePolicy.
        /// </summary>
        /// <typeparam name="T">Object Type and it should be the a Class</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <param name="data">This is the data object which needs to store in Cache.</param>
        /// <param name="cachePolicy">This is the custom cachepolicy which will apply on the provided object of Type T.</param>
        public override void Add<T>(string key, T data, CacheItemPolicy cachePolicy)
        {
            if (String.IsNullOrEmpty(key.Trim()))
            {
                throw new Exception("Key should not be blank.");
            }
            if (data == null)
            {
                throw new NullReferenceException("Data should not be null.");
            }
            if (cachePolicy == null)
            {
                throw new NullReferenceException("Cache Policy should not be null.");
            }
            if (IsAdded(key))
            {
                throw new Exception("Key is already in use.");
            }
            lock (Cache)
            {
                try
                {
                    if (Cache.Contains(key))
                        Cache.Remove(key);
                    Cache.Add(new CacheItem(key, data), cachePolicy);
                }
                catch (Exception)
                {
                    //don't do any thing  
                }
            }

        }

        public override void Add<T>(string key, T data, DateTimeOffset absoluteExpiration)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new NullReferenceException("Key should not be blank.");
            }
            if (data == null)
            {
                throw new NullReferenceException("Data should not be null.");
            }

            lock (Cache)
            {
                try
                {
                    if (Cache.Contains(key))
                        Cache.Remove(key);
                    Cache.Add(key, data, absoluteExpiration);
                }
                catch (Exception ex)
                { _logger.Error(string.Format("Exception in adding cache key :- {0}  ", Utilities.GetDetailedException(ex))); }
            }
        }


        /// <summary>
        /// This method provides an Object of Type 'T' from Cache by uniquely identify it using the provided 'Key'.
        /// </summary>
        /// <typeparam name="T">Object Type where T should be a Class</typeparam>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <returns>The reqired Object which is identified using the provided key.</returns>
        public override T Get<T>(string key)
        {
            if (!IsAdded(key))
            {
                throw new KeyNotFoundException("Key is not avaible in Cache.");
            }
            if (Cache[key] == null)
            {
                throw new Exception("Cache data is Expire.");
            }
            return (T)Cache[key];
        }
        /// <summary>
        /// This method is used to remove the Object from Cache.
        /// </summary>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        public override void Remove(string key)
        {
            if (String.IsNullOrEmpty(key.Trim()))
            {
                throw new Exception("Key should not be blank.");
            }
            try
            {
                if (IsAdded(key))
                {
                    Cache.Remove(key);
                }
            }
            catch (Exception)
            {
                //don't do any thing
            }

        }
        /// <summary>
        /// This method is checks whether the Object is available in the cache or not.
        /// </summary>
        /// <param name="key">Key to uniquely identify the object from Cache.</param>
        /// <returns>True if records exists in Cache which is identified using the provided key.</returns>
        public override Boolean IsAdded(string key)
        {
            if (String.IsNullOrEmpty(key.Trim()))
            {
                throw new Exception("Key should not be blank.");
            }
            return (Cache[key] != null);
        }
        #endregion
    }
}
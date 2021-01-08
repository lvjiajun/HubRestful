using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace HubRestful.Model
{
    public class MemoryCacheExtensions:ICache
    {
        private readonly IMemoryCache _cache;
        /// <summary>
        /// 缓存配置项
        /// </summary>
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        //构造器注入
        public MemoryCacheExtensions(IMemoryCache cache)
        {
             //单项缓存设置项
            _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Low,
                //缓存大小占1份
                Size = 1
            };
            _cache = cache;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheMin"></param>
        /// <param name="obsloteType"></param>
        public void Add<T>(string key, T data, ObsloteType obsloteType = default, int  cacheMin = 30)
        {
            if (obsloteType == ObsloteType.Absolutely)
            {
                _memoryCacheEntryOptions.AbsoluteExpiration =  DateTime.Now.AddSeconds(cacheMin);
            }
            if (obsloteType == ObsloteType.Relative)
            {
                _memoryCacheEntryOptions.SlidingExpiration =  TimeSpan.FromMinutes(cacheMin);
            }
            if (Contains(key))
            {
                Upate<T>(key, data, obsloteType);
            }
            else
            {
                _cache.Set(key, data, _memoryCacheEntryOptions);
            }

        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            object RetValue;
            return _cache.TryGetValue(key, out RetValue);
        }
        
        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }
        /// <summary>
        /// 获取缓存集合
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        public IDictionary<string, object> GetAll(IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            var dict = new Dictionary<string, object>();
            keys.ToList().ForEach(item => dict.Add(item, _cache.Get(item)));
            return dict;
        }
        
        
        public bool Remove(string key)
        {
            bool ReturnBool = true;
            if (!Contains(key))
            {
                ReturnBool = false;
            }
            else {
                _cache.Remove(key);
            }

            return ReturnBool;
        }
        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <returns></returns>
        public void RemoveList(IEnumerable<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            keys.ToList().ForEach(item => _cache.Remove(item));
        }
        public bool Upate<T>(string key,T data, ObsloteType obsloteType, int cacheMin =  30)
        {
            bool ReturnBool = true;
            if (!Contains(key))
            {
                ReturnBool = false;
            }
            else
            {
                _cache.Remove(key);
                Add(key, data, obsloteType, cacheMin);
            }
            return ReturnBool;

        }
        
        
        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        public List<string> GetListKey()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null) return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }
            return keys;
        }
        /// <summary>
        /// 获取所有缓存值
        /// </summary>
        /// <returns></returns>
        public List<T> GetListValue<T>()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<T>();
            if (cacheItems == null) return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(Get<T>(cacheItem.Key.ToString()));
            }
            return keys;
        }
    }
}
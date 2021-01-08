namespace HubRestful.Model
{
    public interface ICache
    {
        /// <summary>
        /// 读取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cachetime"></param>
        void Add<T>(string key, T data, ObsloteType obsloteType = default, int cachetime =  30);
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(string key);
        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="key"></param>
        bool Remove(string key);
        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Upate<T>(string key,T data, ObsloteType obsloteType = default, int cacheMin =  30);
    }
}
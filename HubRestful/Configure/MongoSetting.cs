﻿namespace HubRestful.Configure
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoSetting:IMongoDataBaseSettings
    {
        public string ScanCollectionName { get; set; }
        public string OrderCollectionName { get; set; }
        public string ErrCollectionName { get; set; }
        
        public string TagCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public interface IMongoDataBaseSettings
    {
        string ScanCollectionName { get; set; }
        string OrderCollectionName { get; set; }
        string ErrCollectionName { get; set; }
        string TagCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
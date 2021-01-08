using System;
using System.Collections.Generic;
using HubRestful.Configure;
using HubRestful.Entity;
using HubRestful.Model;
using MongoDB.Driver;

namespace HubRestful.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class TagDataBaseServer
    {
        private readonly IMongoCollection<TagEntity> _tagEntity;
        
        /// <summary>
        /// tag Server 构造函数
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="memoryCacheExtensions"></param>
        public TagDataBaseServer(
            IMongoDataBaseSettings settings
            , MemoryCacheExtensions memoryCacheExtensions)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _tagEntity = database.GetCollection<TagEntity>(settings.TagCollectionName);
        
        }

        public List<TagEntity> Get() =>
            _tagEntity.Find(tagEntity => true).ToList();
        
        public TagEntity Get(string id)
        {
            return _tagEntity.Find<TagEntity>(tagEntity => tagEntity.id == id).FirstOrDefault();
        }
        public List<TagEntity> Get(DateTime startTime, DateTime endTime)
        {
            return _tagEntity.Find(filter => 
                ((filter.ReadTime <= endTime)&&(filter.ReadTime >= startTime))).ToList();
        }


        public TagEntity Create(TagEntity tagEntity)
        {
            _tagEntity.InsertOne(tagEntity);
            return tagEntity;
        }
        
        public void Update(string id, TagEntity tagEntity)
        {
            _tagEntity.ReplaceOne(scanEntity => scanEntity.id == id, tagEntity);
        }

        public void Remove(string id)
        {
            _tagEntity.DeleteOne(tagEntity => tagEntity.id == id);
        }

        public void RemoveList(List<string> idList)
        {
            foreach (var id in idList)
            {
                _tagEntity.DeleteOne(tagEntity => tagEntity.id == id);
            }
        }
    }
}
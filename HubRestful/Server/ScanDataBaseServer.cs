using System;
using System.Collections.Generic;
using HubRestful.Configure;
using HubRestful.Entity;
using MongoDB.Driver;

namespace HubRestful.Model
{
    public class ScanDataBaseServer
    {
        private readonly IMongoCollection<ScanEntity> _scanEntity;
        private readonly MemoryCacheExtensions _memoryCacheExtensions;
        
        public ScanDataBaseServer(
            IMongoDataBaseSettings settings
            , MemoryCacheExtensions memoryCacheExtensions)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _scanEntity = database.GetCollection<ScanEntity>(settings.ScanCollectionName);
            _memoryCacheExtensions = memoryCacheExtensions;
        }

        public List<ScanEntity> Get() =>
            _scanEntity.Find(scanEntity => true).ToList();
        
        public ScanEntity Get(string id)
        {
            return _scanEntity.Find<ScanEntity>(scanEntity => scanEntity.id == id).FirstOrDefault();
        }
        public List<ScanEntity> Get(DateTime startTime, DateTime endTime)
        {
            return _scanEntity.Find(filter => 
                ((filter.scanTime <= endTime)&&(filter.scanTime >= startTime))).ToList();
        }
        public List<ScanEntity> GetCache()
        {
            return _memoryCacheExtensions.GetListValue<ScanEntity>();
        }
        public ScanEntity GetCache(string id)
        {
            return _memoryCacheExtensions.Get<ScanEntity>(id);
        }

        public ScanEntity Create(ScanEntity scanEntity)
        {
            _scanEntity.InsertOne(scanEntity);
            _memoryCacheExtensions.Add<ScanEntity>(scanEntity.scanData, scanEntity,ObsloteType.Absolutely,1800);
            return scanEntity;
        }
        
        public void Update(string id, ScanEntity scanEntity)
        {
            _scanEntity.ReplaceOne(scanEntity => scanEntity.id == id, scanEntity);
            if (_memoryCacheExtensions.Contains(id))
            {
                _memoryCacheExtensions.Upate<ScanEntity>(scanEntity.id, scanEntity, ObsloteType.Absolutely, 1800);
            }
        }

        public void Remove(string id)
        {
            _scanEntity.DeleteOne(scanEntity => scanEntity.id == id);
            if (_memoryCacheExtensions.Contains(id))
            {
                _memoryCacheExtensions.Remove(id);
            }
        }

        public void RemoveList(List<string> idList)
        {
            foreach (var id in idList)
            {
                _scanEntity.DeleteOne(scanEntity => scanEntity.id == id);
                if (_memoryCacheExtensions.Contains(id))
                {
                    _memoryCacheExtensions.Remove(id);
                }
            }
        }

        public void RemoveCache(string id)
        {
            if (_memoryCacheExtensions.Contains(id))
            {
                _memoryCacheExtensions.Remove(id);
            }
        }

        public void RemoveCacheList(List<string> idList)
        {
            foreach (var id in idList)
            {
                if (_memoryCacheExtensions.Contains(id))
                {
                    _memoryCacheExtensions.Remove(id);
                }
            }
        }
    }
}
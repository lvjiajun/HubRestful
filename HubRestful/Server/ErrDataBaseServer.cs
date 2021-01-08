﻿using System;
using System.Collections.Generic;
using HubRestful.Configure;
using HubRestful.Entity;
using MongoDB.Driver;

namespace HubRestful.Model
{
    public class ErrDataBaseServer
    {
        private readonly IMongoCollection<ErrEntity> _errEntity;

        public ErrDataBaseServer(IMongoDataBaseSettings settings
            , MemoryCacheExtensions memoryCacheExtensions)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _errEntity = database.GetCollection<ErrEntity>(settings.ErrCollectionName);
        }
        
        public IEnumerable<ErrEntity> Get(DateTime startTime, DateTime endTime) =>
            _errEntity.Find<ErrEntity>(order => ((order.errTime <= endTime) && (order.errTime >= startTime)))
                .ToList();
    }
}
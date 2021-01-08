using System;
using System.Collections.Generic;
using HubRestful.Configure;
using HubRestful.Entity;
using MongoDB.Driver;

namespace HubRestful.Model
{
    public class OrderDataBaseServer
    {
        private readonly IMongoCollection<OrderEntity> _orderEntity;

        public OrderDataBaseServer(IMongoDataBaseSettings settings
            , MemoryCacheExtensions memoryCacheExtensions)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _orderEntity = database.GetCollection<OrderEntity>(settings.OrderCollectionName);
        }

        public IEnumerable<OrderEntity> Get()=> 
            _orderEntity.Find(scanEntity => true).ToList();
        
        public OrderEntity Get(string id)=> 
            _orderEntity.Find<OrderEntity>(order => order.id == id).FirstOrDefault();

        public IEnumerable<OrderEntity> Get(DateTime startTime, DateTime endTime)
        {
            DateTime dataTime = endTime;
            if (endTime.Year == 1)
            {
                dataTime = DateTime.Now;
            }
            if (endTime.Subtract(startTime).TotalDays > 30)
            {
                return _orderEntity.Find<OrderEntity>(order => ((order.scanTime <= dataTime) && (order.scanTime >= startTime)))
                    .ToList();
            }else
            {
                return null;
            }
        }

        public OrderEntity Create(OrderEntity orderEntity)
        {
            _orderEntity.InsertOne(orderEntity);
            return orderEntity;
        }

        public void Update(OrderEntity orderEntity)=>
            _orderEntity.ReplaceOne(orderEntity.id, orderEntity);

        public void Remove(string id) => _orderEntity.DeleteOne(id);

        public void Remove(IEnumerable<string> idList)
        {
            foreach (var id in idList)
            {
                _orderEntity.DeleteOne(order => order.id == id);
            }
        }
    }
}
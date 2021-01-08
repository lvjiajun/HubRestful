using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HubRestful.Entity
{
    public class OrderEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        
        public DateTime scanTime { get; set; }
    }
}
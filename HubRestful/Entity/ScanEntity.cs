using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace HubRestful.Entity
{
    public class ScanEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public string id { get; set; }
        
        public DateTime scanTime { get; set; }
       
        public string scanData { get; set; }
    }
}
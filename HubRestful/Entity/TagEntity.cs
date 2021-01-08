using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HubRestful.Entity
{
    /// <summary>
    /// Tag标签
    /// </summary>
    public class TagEntity
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public string id { get; set; }
        /// <summary>
        /// 工作中心
        /// </summary>
        public string LineCode { get; set; }
        
        /// <summary>
        /// 台站号
        /// </summary>
        public string WorkUnit { get; set; }
        
        /// <summary>
        /// 遥控器条码
        /// </summary>
        public string RFIDNum { get; set; }

        /// <summary>
        /// 芯片ID
        /// </summary>
        public string RFIDCode { get; set; }
        
        /// <summary>
        /// MES条码	
        /// </summary>
        public string MesCode { get; set; }
        
        /// <summary>
        /// 机型条码
        /// </summary>
        public string BarCode { get; set; }
        
        /// <summary>
        /// 读卡时间
        /// </summary>
        public DateTime ReadTime { get; set; }
    }
}
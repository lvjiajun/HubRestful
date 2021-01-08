using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HubRestful.Entity
{
    public class TaskEntity
    {
        [JsonPropertyName("MESORDERNO")]
        [JsonProperty("MESORDERNO")]
        public string mesOrderId { get; set; }
        
        [JsonPropertyName("物料编码")]
        [JsonProperty("物料编码")]
        public string itemCodes { get; set; }
        
        [JsonPropertyName("容器ID")]
        [JsonProperty("容器ID")]
        public string buckerId { get; set; }
        
        [JsonPropertyName("库位")]
        [JsonProperty("库位")]
        public string location { get; set; }
        
        [JsonPropertyName("数量")]
        [JsonProperty("数量")]
        public double amount { get; set; }
        
        [JsonPropertyName("创建时间")]
        [JsonProperty("创建时间")]
        public DateTime timeStamp { get; set; }
    }
}
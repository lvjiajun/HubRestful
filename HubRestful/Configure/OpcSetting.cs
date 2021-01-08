namespace HubRestful.Configure
{
    /// <summary>
    /// 
    /// </summary>
    public class OpcSetting:IOpcSetting
    {
        /// <summary>
        /// 
        /// </summary>
        public string endpointUrl { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool autoAccept { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int stopTimeout { get; set; }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IOpcSetting
    {
        /// <summary>
        /// 
        /// </summary>
        string endpointUrl { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        bool autoAccept { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int stopTimeout { get; set; }
    }
}
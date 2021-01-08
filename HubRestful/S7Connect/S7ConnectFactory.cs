
using System;
using HubRestful.Configure;
using Microsoft.Extensions.Logging;
using S7.Net;
namespace HubRestful
{
    /// <summary>
    /// 
    /// </summary>
    public interface IS7ConnectFactory
    {
        /// <summary>
        /// 关闭S7连接
        /// </summary>
        public void close();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Plc GetService();
    }
    /// <summary>
    /// 
    /// </summary>
    public class S7ConnectFactory:IS7ConnectFactory
    {
        public Plc Plc;
        
        private readonly IS7setting _setting;
        private readonly ILogger<S7ConnectFactory> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        public S7ConnectFactory(IS7setting setting, ILogger<S7ConnectFactory> logger)
        {
            _setting = setting;
            
            _logger = logger;
            
            Plc = new Plc(CpuType.S7200,_setting.Url, (Int16)_setting.Rack, (Int16)_setting.Slot);
            
            _logger.LogInformation("This S7 has connect url:{0},rack:{1},slot:{2}",
                
                _setting.Url,_setting.Rack,_setting.Slot);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void close()
        {
            Plc.Close();
            
            _logger.LogWarning("This S7 has connect url:{0},rack:{1},slot:{2}",
                
                _setting.Url,_setting.Rack,_setting.Slot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Plc GetService()
        {
            return Plc;
        }
    }
}
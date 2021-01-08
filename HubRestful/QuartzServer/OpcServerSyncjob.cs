using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using S7.Net;

namespace HubRestful.QuartzServer
{
    public class OpcServerSyncjob : IJob
    {
        private readonly ILogger<OpcServerSyncjob> _logger;
        private readonly IS7ConnectFactory _s7ConnectFactory;
        
        private Plc Plc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="s7ConnectFactory"></param>
        public OpcServerSyncjob(ILogger<OpcServerSyncjob> logger, IS7ConnectFactory s7ConnectFactory)
        {
            _logger = logger;
            _s7ConnectFactory = s7ConnectFactory;
            Plc = _s7ConnectFactory.GetService();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {  

                _logger.LogInformation ($"{DateTime.Now.ToString()}：开始执行同步第三方数据");
                //  我们都知道如果一个从构造方法中获取IOC容器里面的类型，必须该类型也要主要到IOC容器中；
                
            });
        }
    }
}
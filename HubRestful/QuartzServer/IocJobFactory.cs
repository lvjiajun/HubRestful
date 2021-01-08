using System;
using Quartz;
using Quartz.Spi;

namespace HubRestful.QuartzServer
{
    public class IocJobFactory:IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public IocJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;      
        }
        
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetService(bundle.JobDetail.JobType) as IJob; 
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
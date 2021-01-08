using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HubRestful.QuartzServer
{
    public class StartJob : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await new TaskFactory().StartNew(() =>
                {
                    //定时任务休眠
                    Thread.Sleep(1 * 1000);
                });
            }
        }
    }
}
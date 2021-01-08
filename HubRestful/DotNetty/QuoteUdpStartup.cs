using System;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using HubRestful.QuartzServer;
using Microsoft.Extensions.Logging;


namespace HubRestful.DotNetty
{
    /// <summary>
    /// 
    /// </summary>
    public class QuoteUdpStartup
    {
        private readonly ILogger<QuartzStartup> _logger;
        private readonly QuoteOfTheMomentClientHandler _quoteOfTheMomentClientHandler;
        private IChannel _clientChannel;
        
        /// <summary>
        /// UDP启动的构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="quoteOfTheMomentClientHandler"></param>
        public QuoteUdpStartup(ILogger<QuartzStartup> logger,QuoteOfTheMomentClientHandler quoteOfTheMomentClientHandler)
        {
            _logger = logger;
            _quoteOfTheMomentClientHandler = quoteOfTheMomentClientHandler;
        }

        public async Task Start()
        {
            
            var group = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<SocketDatagramChannel>()
                    .Option(ChannelOption.SoBroadcast, true)
                    .Handler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        channel.Pipeline.AddLast("Quote", _quoteOfTheMomentClientHandler);
                    }));

                _clientChannel = await bootstrap.BindAsync(3304);
                _logger.LogInformation("Dot Netty UDP RFID have Start");
            }
            catch (Exception e)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }
        
        public void Stop()
        {
            _clientChannel.CloseAsync();
            _logger.LogInformation("Dotnetty job upload as application stopped");
        }
    }
}
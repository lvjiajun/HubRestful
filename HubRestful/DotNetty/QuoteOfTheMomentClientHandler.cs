using System;
using System.Collections.Concurrent;
using System.Text;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using HubRestful.Entity;
using HubRestful.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HubRestful.DotNetty
{
    /// <summary>
    /// 
    /// </summary>
    public class QuoteOfTheMomentClientHandler : SimpleChannelInboundHandler<DatagramPacket>
    {
        /// <summary>
        /// 阻塞队列
        /// </summary>
        public BlockingCollection<TagEntity> tagEntities { get; set; }

        private readonly TagDataBaseServer _tagDataBaseServer;
        
        private readonly ILogger<QuoteOfTheMomentClientHandler> _logger;
        /// <summary>
        /// Netty接受函数的注入构造函数
        /// </summary>
        /// <param name="logger"></param>
        public QuoteOfTheMomentClientHandler(ILogger<QuoteOfTheMomentClientHandler> logger, TagDataBaseServer tagDataBaseServer)
        {
            tagEntities = new BlockingCollection<TagEntity>(8192);
            _logger = logger;
            _tagDataBaseServer = tagDataBaseServer;
        }
        /// <summary>
        /// 接受函数的回调函数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="packet"></param>
        protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket packet)
        {
           _logger.LogInformation($"Client Received => {packet}");
            if (!packet.Content.IsReadable())
            {
                return;
            }
            try
            {
                TagEntity tagEntity = JsonConvert.DeserializeObject<TagEntity>(packet.Content.ToString(Encoding.UTF8));
                _tagDataBaseServer.Create(tagEntity);
                
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        /// <summary>
        /// UDP接受消息异常
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError("Exception: " + exception);
            context.CloseAsync();
        }
    }
}
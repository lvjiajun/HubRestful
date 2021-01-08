using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HubRestful.DotNetty;
using HubRestful.Entity;
using HubRestful.OpcClientServer;
using HubRestful.Server;
using Microsoft.AspNetCore.Mvc;

namespace HubRestful.Controllers
{
    /// <summary>
    /// Tag标签查询接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly TagDataBaseServer _tagDataBaseServer;
        public BlockingCollection<TagEntity> tagEntities { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagDataBaseServer"></param>
        /// <param name="quoteOfTheMomentClientHandler"></param>
        public TagController(TagDataBaseServer tagDataBaseServer, QuoteOfTheMomentClientHandler quoteOfTheMomentClientHandler)
        {
            _tagDataBaseServer = tagDataBaseServer;
            tagEntities = quoteOfTheMomentClientHandler.tagEntities;
        }

        // GET: api/<TaskController>
        /// <summary>
        /// 推送工业板信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public TagEntity Creat([FromBody] TagEntity tagEntity)
        {
            tagEntities.Add(tagEntity);
            return _tagDataBaseServer.Create(tagEntity);
        }

        /// <summary>
        /// 时间查询RFID标签数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("time")]
        public List<TagEntity> Get(DateTime startTime,DateTime endTime)
        {
            return _tagDataBaseServer.Get(startTime, endTime);
        }

        /// <summary>
        /// 查询工装板消息阻塞
        /// </summary>
        /// <returns></returns>
        [HttpGet("reset")]
        public TagEntity GetList()
        {
            TagEntity tagEntity;
            tagEntities.TryTake(out tagEntity, 1000);
            return tagEntity;
        }
        
        /// <summary>
        /// 获取一个小时内的数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<TagEntity> Get()
        {
            return _tagDataBaseServer.Get(DateTime.Now.AddHours(-1), DateTime.Now);
        }
    }
}
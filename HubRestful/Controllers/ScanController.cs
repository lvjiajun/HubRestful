using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubRestful.DotNetty;
using HubRestful.Entity;
using HubRestful.Model;
using HubRestful.OpcClientServer.Client;
using HubRestful.RestClient;
using Microsoft.Extensions.Logging;
using Refit;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HubRestful.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        
        private readonly ScanDataBaseServer _scanDataBaseServer;
        private readonly ILogger<ScanController> _logger;
        public ScanController(
            ScanDataBaseServer scanDataBaseServer,
            ILogger<ScanController> logger)
        {
           // _opcuaClient = opcuaClient;
            _scanDataBaseServer = scanDataBaseServer;
            _logger = logger;
        }


        // GET: api/<ScanController>
        /// <summary>
        /// 获取混流线缓存数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<ScanEntity> Get()
        {
            IEnumerable<ScanEntity> scanEntities = _scanDataBaseServer.GetCache();
            return scanEntities;
        }
        
        // GET: api/<ScanController>
        /// <summary>
        /// 获取混流线300s数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("time")]
        public IEnumerable<ScanEntity> GetTime()
        {
            return GetTime(DateTime.Today, DateTime.Now);
        }
            
       
        
        // GET: api/<ScanController>
        /// <summary>
        /// 获取混流线扫描历史数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("history")]
        public IEnumerable<ScanEntity> GetTime(DateTime startTime, DateTime endTime)=>
            _scanDataBaseServer.Get(startTime, endTime);

        
        // GET: api/<ScanController>
        /// <summary>
        /// 查询扫描数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:length(24)}")]
        public ActionResult<ScanEntity> Get(string id)
        { 
            var scanEntity = _scanDataBaseServer.Get(id);
        
            if (scanEntity == null)
            {
                return NotFound();
            }
        
            return scanEntity;
        }
        
        // GET: api/<ScanController>
        /// <summary>
        /// 添加扫描数据测试用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<ScanEntity> Post([Body]ScanEntity scanEntity)
        {
            scanEntity.scanTime = DateTime.Now;
            return _scanDataBaseServer.Create(scanEntity);
        }
        
        // DELETE api/<ScanController>/5
        /// <summary>
        /// 删除扫描数据测试用
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public void Delete(List<string> id)
        {
            _scanDataBaseServer.RemoveCacheList(id);
        }
        
        // GET: api/<ScanController>
        /// <summary>
        /// 获取混流线扫描所有数据测试用
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public IEnumerable<ScanEntity> GetAll() =>
            _scanDataBaseServer.Get();
    }
}

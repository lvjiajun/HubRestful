using System;
using System.Collections.Generic;
using HubRestful.Entity;
using HubRestful.Model;
using Microsoft.AspNetCore.Mvc;

namespace HubRestful.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly OrderDataBaseServer _orderDataBaseServer;

        public OrderController(OrderDataBaseServer orderDataBaseServer)
        {
            _orderDataBaseServer = orderDataBaseServer;
        }

        // GET: api/<ScanController>
        /// <summary>
        /// 查询订单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<OrderEntity> Get() =>
            _orderDataBaseServer.Get();

        // GET: api/<ScanController>
        /// <summary>
        /// 推送订单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<OrderEntity> Create([FromBody] OrderEntity orderEntity)
        {
            orderEntity.scanTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            return _orderDataBaseServer.Create(orderEntity);
        }
        
        // GET: api/<ScanController>
        /// <summary>
        /// 更新订单信息
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public void Updata([FromBody] OrderEntity orderEntity)
        {
            _orderDataBaseServer.Update(orderEntity);
        }
    }
}
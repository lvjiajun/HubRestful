﻿using System;
using System.Collections.Generic;
using HubRestful.Entity;
using HubRestful.Model;
using Microsoft.AspNetCore.Mvc;

namespace HubRestful.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrController : ControllerBase
    {

        private readonly ErrDataBaseServer _errDataBaseServer;

        public ErrController(ErrDataBaseServer errDataBaseServer)
        {
            _errDataBaseServer = errDataBaseServer;
        }

        // GET: api/<ScanController>
        /// <summary>
        /// 查询错误
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<ErrEntity> GetTime(DateTime startTime, DateTime endTime)=>
            _errDataBaseServer.Get(startTime, endTime);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubRestful.Entity;
using HubRestful.RestClient;
using Microsoft.AspNetCore.Mvc;

namespace HubRestful.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IRestfulClient _restfulClient;

        public TaskController(IRestfulClient restfulClient)
        {
            _restfulClient = restfulClient;
        }
        // GET: api/<TaskController>
        /// <summary>
        /// 获取MES任务数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<TaskEntity>> Get(DateTime start,DateTime end)
        {
            return await _restfulClient.GetTask(start, end);
        }
        // GET: api/<TaskController>
        /// <summary>
        /// 模拟MES任务数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public List<TaskEntity> GetTime(DateTime dateAfterStr, DateTime dateBeforStr)
        {
            TaskEntity taskEntity = new TaskEntity
            {
                mesOrderId = "SD123231",
                itemCodes = "21212",
                buckerId = "A234234",
                location = " location",
                amount = 12,
                timeStamp = DateTime.Now
            };
            TaskEntity taskEntits = new TaskEntity
            {
                mesOrderId = "ss23"
            };
            List<TaskEntity> taskEntities = new List<TaskEntity>();
            taskEntities.Add(taskEntity);
            return taskEntities;
        }
    }
}
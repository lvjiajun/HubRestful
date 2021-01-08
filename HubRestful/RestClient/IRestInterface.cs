using HubRestful.Entity;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubRestful.RestClient
{
    [Headers("User-Agent:Gree")]
    public interface IRestInterface
    {
        [Get("/api/Task/test")]
        Task<List<TaskEntity>> GetTask(string dateBeforStr,string dateAfterStr);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HubRestful.Entity;
using Refit;


namespace HubRestful.RestClient
{
    public interface IRestfulClient
    {
        public Task<List<TaskEntity>> GetTask(DateTime startTime, DateTime endTime);

    }
}
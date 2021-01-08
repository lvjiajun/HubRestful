using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using HubRestful.Entity;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using Refit;
using Ubiety.Dns.Core;

namespace HubRestful.RestClient
{
    public class RestfulClient:IRestfulClient
    {
        private readonly IRestInterface _restInterface;
        private readonly ILogger<RestfulClient> _logger;
        private const string From = "Dms After Sales";
        
        public RestfulClient(IRestInterface restInterface,ILogger<RestfulClient> logger)
        {
            _restInterface = restInterface;
            _logger = logger;
        }
        
        private async Task<Exception> WrapException(ApiException exception) {
            if (exception.StatusCode == System.Net.HttpStatusCode.BadRequest) {
                var receivedBody = await exception.GetContentAsAsync<Exception>();
                return new ValidationException($"业务校验失败，{receivedBody.Message} ({From})", exception);
            } else {
                _logger.LogWarning(exception, "Call Dms After Sales API failed");
                return new ApplicationException($"内部调用失败，{exception.Message} ({exception.StatusCode}) ({From})", exception);
            }
        }
        private Exception WrapException(HttpRequestException exception) {
            _logger.LogWarning(exception, "Call Dms After Sales API failed");
            return new ApplicationException($"内部调用失败，{exception.Message} ({From})", exception);
        }
        
        public async Task<List<TaskEntity>> GetTask(DateTime startTime,DateTime endTime) {
            try {
                
                return await _restInterface.GetTask(
                    
                    startTime.ToString("yyyy/MM/dd HH:mm:ss"),
                    
                    endTime.ToString("yyyy/MM/dd HH:mm:ss"));
                
            } catch (ApiException ex) {
                throw await WrapException(ex);
            } catch (HttpRequestException ex) {
                throw WrapException(ex);
            }
        }
    }
}

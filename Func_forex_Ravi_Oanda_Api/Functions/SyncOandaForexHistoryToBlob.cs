using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Functions
{
    public class SyncOandaForexHistoryToBlob
    {
        private readonly ILogger<SyncOandaForexHistoryToBlob> log;
        private readonly IOandaApi oandaApi;
        private readonly BlobStorageTableHelpers tableHelpers;

        public SyncOandaForexHistoryToBlob(ILogger<SyncOandaForexHistoryToBlob> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new BlobStorageTableHelpers();
        }

        [FunctionName("SyncOandaForexHistoryToBlob")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Provide Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oanda/forex/history/sync")] HttpRequest req)
        {
            log.LogInformation("SyncOandaForexHistoryToBlob function received an request.");
            string[] instruments = req.Query["instrument"].ToString().Split(",");
            var accountDetail = await oandaApi.GetAccount();
            foreach (var instrument in instruments)
            {
                var priceHistory = await oandaApi.GetPriceHistory(instrument);
                if (priceHistory != null && priceHistory.Candles != null && priceHistory.Candles.Any())
                {
                    var tableData = priceHistory.TransformHistDataToFxCurrentTable(accountDetail.account);
                    await tableHelpers.InsertEntityAsync(tableData);
                }
            }
            return new OkObjectResult("done");
        }
    }
}


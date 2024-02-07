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
        private readonly FxCurrencyTableHelpers tableHelpers_15;
        private readonly FxCurrencyTableHelpers tableHelpers_5;

        public SyncOandaForexHistoryToBlob(ILogger<SyncOandaForexHistoryToBlob> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers_15 = new FxCurrencyTableHelpers("oandaforexdatademo");
            tableHelpers_5 = new FxCurrencyTableHelpers("oandaforexfivemindatademo");
        }

        [FunctionName("SyncOandaForexHistoryToBlob_15min")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "SyncOandaForexHistoryToBlob_15min" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Provide Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oanda/forex/history/sync/15")] HttpRequest req)
        {
            log.LogInformation("SyncOandaForexHistoryToBlob_15min function received an request.");
            string[] instruments = req.Query["instrument"].ToString().Split(",");
            var accountDetail = await oandaApi.GetAccount();
            foreach (var instrument in instruments)
            {
                var priceHistory = await oandaApi.GetPriceHistory15Min(instrument);
                if (priceHistory != null && priceHistory.Candles != null && priceHistory.Candles.Any())
                {
                    var tableData = priceHistory.TransformHistDataToFxCurrentTable(accountDetail.account);
                    await tableHelpers_15.InsertEntityAsync(tableData);
                }
            }
            return new OkObjectResult("done");
        }

        [FunctionName("SyncOandaForexHistoryToBlob_5min")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "SyncOandaForexHistoryToBlob_5min" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Provide Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oanda/forex/history/sync/5")] HttpRequest req)
        {
            log.LogInformation("SyncOandaForexHistoryToBlob_5min function received an request.");
            string[] instruments = req.Query["instrument"].ToString().Split(",");
            var accountDetail = await oandaApi.GetAccount();
            foreach (var instrument in instruments)
            {
                var priceHistory = await oandaApi.GetPriceHistory5Min(instrument);
                if (priceHistory != null && priceHistory.Candles != null && priceHistory.Candles.Any())
                {
                    var tableData = priceHistory.TransformHistDataToFxCurrentTable(accountDetail.account);
                    await tableHelpers_5.InsertEntityAsync(tableData);
                }
            }
            return new OkObjectResult("done");
        }
    }
}


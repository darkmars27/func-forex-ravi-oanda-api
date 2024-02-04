using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;

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

        [FunctionName("AddInstrumentHistoryFunction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Provide Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oanda/forex/history/sync")] HttpRequest req)
        {
            log.LogInformation("AddInstrumentHistoryFunction function received an request.");
            string name = req.Query["instrument"];

            var priceHistory = await oandaApi.GetPriceHistory(Constants.EUR_USD);
            var accountDetail = await oandaApi.GetAccount();

            if (priceHistory!= null && priceHistory.Candles != null && priceHistory.Candles.Any())
            {
                var tableData = priceHistory.TransformHistDataToFxCurrentTable(accountDetail.account);
                await tableHelpers.InsertEntityAsync(tableData);
                return new OkObjectResult("done");
            }
            else
            {
                return new OkObjectResult("no data found");
            }
            
        }
    }
}


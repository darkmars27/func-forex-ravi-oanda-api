using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Functions
{
    public class ManualDataSyncFunction
    {
        private readonly ILogger<ManualDataSyncFunction> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        private readonly ILoggerFactory loggerFactory;
        private readonly StoreInstrument storeInstrument;
        private readonly TradeInstrument tradeInstrument;

        public ManualDataSyncFunction(ILogger<ManualDataSyncFunction> log, IOandaApi oandaApi, ILoggerFactory loggerFactory)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            this.loggerFactory = loggerFactory;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
            storeInstrument = new StoreInstrument(loggerFactory.CreateLogger<StoreInstrument>(), oandaApi);
            tradeInstrument = new TradeInstrument(loggerFactory.CreateLogger<TradeInstrument>(), oandaApi);
        }

        [FunctionName("ManualDataSyncFunction")]
        [OpenApiOperation(operationId: "ManualDataSyncFunction", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument_name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Supply Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "ManualDataSyncFunction")] HttpRequest req)
        {
            var instrument_name = req.Query["instrument_name"];
            log.LogInformation($"QuarterHourForexTradingFunction function executed at: {DateTime.Now}");

            await storeInstrument.Run(instrument_name);
            await tradeInstrument.Run(instrument_name);
            return new OkObjectResult("done");
        }
    }
}


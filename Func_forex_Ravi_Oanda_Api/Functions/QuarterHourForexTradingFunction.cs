using System;
using System.Linq;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Functions;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Func_forex_Ravi_Oanda_Api.Functions
{
    public class QuarterHourForexTradingFunction
    {
        private readonly ILogger<QuarterHourForexTradingFunction> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        private readonly ILoggerFactory loggerFactory;
        private readonly StoreInstrument storeInstrument;
        private readonly TradeInstrument tradeInstrument;

        public QuarterHourForexTradingFunction(ILogger<QuarterHourForexTradingFunction> log, IOandaApi oandaApi, ILoggerFactory loggerFactory)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            this.loggerFactory = loggerFactory;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
            storeInstrument = new StoreInstrument(loggerFactory.CreateLogger<StoreInstrument>(), oandaApi);
            tradeInstrument = new TradeInstrument(loggerFactory.CreateLogger<TradeInstrument>(), oandaApi);
        }

        [FunctionName("QuarterHourForexTradingFunction")]
        public async Task Run([TimerTrigger("0/15 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"QuarterHourForexTradingFunction function executed at: {DateTime.Now}");
                var fxdata = await storeInstrument.Run(Constants.EUR_USD);
                await tradeInstrument.Run(Constants.EUR_USD, fxdata);
            }
            catch (Exception ex)
            {
                log.LogError($"QuarterHourForexTradingFunction failed with error : {ex.ToString()}");
            }
        }
    }
}

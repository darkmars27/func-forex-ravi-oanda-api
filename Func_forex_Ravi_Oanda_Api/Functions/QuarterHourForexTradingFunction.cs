using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Functions;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
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
        private readonly ILoggerFactory loggerFactory;
        private readonly StoreInstrument storeInstrument;
        private readonly TradeInstrument tradeInstrument;


        public QuarterHourForexTradingFunction(ILogger<QuarterHourForexTradingFunction> log, IOandaApi oandaApi, ILoggerFactory loggerFactory)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            this.loggerFactory = loggerFactory;
            var fxTradeTableHelpers = new FxTradeTableHelpers();
            storeInstrument = new StoreInstrument(loggerFactory.CreateLogger<StoreInstrument>(), oandaApi);
            tradeInstrument = new TradeInstrument(loggerFactory.CreateLogger<TradeInstrument>(), oandaApi, fxTradeTableHelpers);
        }

        [FunctionName("QuarterHourForexTradingFunction")]
        public async Task Run([TimerTrigger("0/15 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"QuarterHourForexTradingFunction function executed at: {DateTime.Now}");

                // Trading EUR_USD
                var fxdata = await storeInstrument.Run(Constants.EUR_USD);
                await tradeInstrument.Run(Constants.EUR_USD, fxdata);

                // Trading AUD_USD
                fxdata = new List<FxCurrencyTable>();
                fxdata = await storeInstrument.Run(Constants.AUD_USD);
                await tradeInstrument.Run(Constants.AUD_USD, fxdata);

                // Trading GBP_USD
                fxdata = new List<FxCurrencyTable>();
                fxdata = await storeInstrument.Run(Constants.GBP_USD);
                await tradeInstrument.Run(Constants.GBP_USD, fxdata);
            }
            catch (Exception ex)
            {
                log.LogError($"QuarterHourForexTradingFunction failed with error : {ex.ToString()}");
            }
        }
    }
}

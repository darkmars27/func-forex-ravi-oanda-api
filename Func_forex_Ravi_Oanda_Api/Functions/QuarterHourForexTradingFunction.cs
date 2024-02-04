using System;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Functions;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.Services.impl;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Func_forex_Ravi_Oanda_Api.Functions
{
    public class RealTimeForexTradingFunction
    {
        private readonly ILogger<RealTimeForexTradingFunction> log;
        private readonly IOandaApi oandaApi;
        private readonly BlobStorageTableHelpers tableHelpers;


        public RealTimeForexTradingFunction(ILogger<RealTimeForexTradingFunction> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new BlobStorageTableHelpers();
        }

        [FunctionName("RealTimeForexTradingFunction")]
        public async Task Run([TimerTrigger("* */15 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RealTimeForexTradingFunction function executed at: {DateTime.Now}");

            var accountDetail = await oandaApi.GetAccount();

            // Sync EUR_USD

            if (accountDetail.account.openTradeCount > 0)
            {
                var open_trade_instrument_name = accountDetail.account.trades[0].instrument;
                var instrument_latestPrice = await oandaApi.GetLatestPrice(open_trade_instrument_name);
                if (instrument_latestPrice != null)
                {
                    var tableRows = instrument_latestPrice.TransformQuarterHourDataToFxCurrentTable(accountDetail.account);
                    foreach(var row in tableRows)
                    {
                        var previous_14_rows = tableHelpers.GetPreviousEntity(row.PartitionKey, row.CurrencyDateTimeUTC.Value, 14);
                        (row.RSI_14, row.Avg_Current_Gain_14, row.Avg_Current_Loss_14) = row.CalculateAlgorithms(previous_14_rows);
                    }                 
                }
            }
            else
            {

            }



                accountDetail = new Currency(log, "EUR_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency(log, "GBP_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency(log, "AUD_USD").GetLatestPrice_buy_sell(accountDetail).Result;
        }
    }
}

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
    public class QuarterHourForexTradingFunction
    {
        private readonly ILogger<QuarterHourForexTradingFunction> log;
        private readonly IOandaApi oandaApi;
        private readonly BlobStorageTableHelpers tableHelpers;


        public QuarterHourForexTradingFunction(ILogger<QuarterHourForexTradingFunction> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new BlobStorageTableHelpers();
        }

        [FunctionName("QuarterHourForexTradingFunction")]
        public async Task Run([TimerTrigger("5 */15 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"QuarterHourForexTradingFunction function executed at: {DateTime.Now}");
            var accountDetail = await oandaApi.GetAccount();
            var eurusd_latestPrice = await oandaApi.GetLatestPrice(Constants.EUR_USD);
            var eurusd_tableData = eurusd_latestPrice.TransformRealTimeDataToFxCurrentTable(accountDetail.account);



            var audusd_latestPrice = await oandaApi.GetLatestPrice(Constants.AUD_USD);
            var gbpusd_latestPrice = await oandaApi.GetLatestPrice(Constants.GBP_USD);



            // Sync EUR_USD
            // var accountDetail = await oandaApi.GetAccount();
            //if (accountDetail.account.openTradeCount > 0)
            //{
            //    var open_trade_instrument_name = accountDetail.account.trades[0].instrument;
            //    var instrument_latestPrice = await oandaApi.GetLatestPrice(open_trade_instrument_name);
            //    if (instrument_latestPrice != null)
            //    {
            //        var tableData = instrument_latestPrice.TransformRealTimeDataToFxCurrentTable(accountDetail.account);
            //    }
            //}
            //else
            //{

            //}



            accountDetail = new Currency(log, "EUR_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency(log, "GBP_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency(log, "AUD_USD").GetLatestPrice_buy_sell(accountDetail).Result;
        }
    }
}

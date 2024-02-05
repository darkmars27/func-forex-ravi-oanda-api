using System;
using System.Linq;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Functions;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
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
        public async Task Run([TimerTrigger("* */15 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"QuarterHourForexTradingFunction function executed at: {DateTime.Now}");

            var accountDetail = await oandaApi.GetAccount();
            var latesPrices = await oandaApi.GetLatestPrice(Constants.EUR_USD, Constants.AUD_USD, Constants.GBP_USD);
            foreach (var instrument in latesPrices.LatestCandles)
            {
                var get_previous_rows = tableHelpers.GetPreviousEntities(instrument.Instrument, 50);
                var tableData = instrument.TransformQuarterHourDataToFxCurrentTable(accountDetail.account, get_previous_rows);
                await tableHelpers.UpsertEntityAsync(tableData);
            }
        }
    }
}

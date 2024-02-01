using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Ravi.Oanda.Automation
{
    public class OandaTimerTrigger
    {
        [FunctionName("OandaTimerTrigger")]
        public void Run([TimerTrigger("10 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var accountDetail = new OandaApi().GetAccount().Result;
            accountDetail = new Currency("EUR_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency("GBP_USD").GetLatestPrice_buy_sell(accountDetail).Result;
            accountDetail = new Currency("AUD_USD").GetLatestPrice_buy_sell(accountDetail).Result;  
        }
    }
}

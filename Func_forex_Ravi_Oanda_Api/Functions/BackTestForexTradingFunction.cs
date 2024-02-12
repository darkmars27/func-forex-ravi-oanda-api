using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Functions;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic;
using Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Func_forex_Ravi_Oanda_Api.Functions
{
    public class BackTestForexTradingFunction
    {
        private readonly ILogger<BackTestForexTradingFunction> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        private readonly ILoggerFactory loggerFactory;
        private readonly StoreInstrument storeInstrument;
        private readonly TradeInstrument tradeInstrument;

        public BackTestForexTradingFunction(ILogger<BackTestForexTradingFunction> log, IOandaApi oandaApi, ILoggerFactory loggerFactory)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            this.loggerFactory = loggerFactory;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
            storeInstrument = new StoreInstrument(loggerFactory.CreateLogger<StoreInstrument>(), oandaApi);
            tradeInstrument = new TradeInstrument(loggerFactory.CreateLogger<TradeInstrument>(), oandaApi);
        }

        [FunctionName("BackTestForexTradingFunction")]
        [OpenApiOperation(operationId: "BackTestForexTradingFunction", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "instrument_name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Supply Instrument Name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "BackTestForexTradingFunction")] HttpRequest req)
        {
            try
            {
                var instrument_name = req.Query["instrument_name"];
                log.LogInformation($"BackTestForexTradingFunction function executed at: {DateTime.Now}");
                var fxdata = tableHelpers.GetPreviousEntities(instrument_name, 10000);
                fxdata = fxdata.OrderByDescending(o => o.RowKey).ToList();

                DateTime dateTime = DateTime.ParseExact("02/01/2024","MM/dd/yyyy",CultureInfo.InvariantCulture);
                var dt_ticks = (DateTime.MaxValue.Ticks - dateTime.Ticks);
                fxdata = fxdata.Where(o => long.Parse(o.RowKey) < dt_ticks).ToList();
                //fxdata = fxdata.Where(o => o.RowKey == "2516945795999999999" || o.RowKey == "2516945804999999999" || o.RowKey == "2516945813999999999").ToList();

                bool isBrought = false;
                DateTimeOffset? buyTime = DateTime.MinValue;
                decimal buyPrice = 0;
                decimal pip = 0.0001m;
                decimal trailingStopPrice = 0;

                for (int i = 1; i < fxdata.Count; i++)
                {
                    var current = fxdata[i];
                    var previous = fxdata[i - 1];
                    current = current.DoEMATrendAnalysis(previous);

                    if (isBrought)
                    {
                        var newtrailingStopPrice = current.Price_Close - (15 * pip);
                        if (newtrailingStopPrice > trailingStopPrice)
                        {
                            trailingStopPrice = newtrailingStopPrice;
                            log.LogInformation($"{current.CurrencyDateTimeEST} - {instrument_name} Changing Trailing Stop Price To {trailingStopPrice}");
                        }

                        if (current.Price_Low <= trailingStopPrice)
                        {
                            log.LogWarning($"{current.CurrencyDateTimeEST} - {instrument_name} Trade Closed (Trailing Stop Price) - Buy:Sell:Profit: {buyPrice}:{trailingStopPrice}:{trailingStopPrice - buyPrice}");
                            isBrought = false;
                        }
                        //if (current.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault() ||
                        //   current.EMA_10_Crossed_EMA_50_From_Above.GetValueOrDefault() ||
                        //   current.EMA_5_Crossed_EMA_50_From_Above.GetValueOrDefault() ||
                        //   Converters.GetCurrentEasternTime().Hour == 7)
                        //{
                        //    log.LogInformation($"{current.CurrencyDateTimeEST} - {instrument_name} Trade Closed - Buy:Sell:Profit: {buyPrice}:{current.Bid_Close}:{current.Bid_Close - buyPrice} - Close Price: {current.Price_Close}");
                        //    isBrought = false;
                        //}

                        else if (current.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault())
                        {
                            log.LogWarning($"{current.CurrencyDateTimeEST} - {instrument_name} Trade Closed EMA_5_10 - Buy:Sell:Profit: {buyPrice}:{current.Bid_Close}:{current.Bid_Close - buyPrice} - Close Price: {current.Price_Close}");
                            isBrought = false;
                        }
                        else if(current.RSI_14 >=70)
                        {
                            log.LogWarning($"{current.CurrencyDateTimeEST} - {instrument_name} Trade Closed RSI_14 - Buy:Sell:Profit: {buyPrice}:{current.Bid_Close}:{current.Bid_Close - buyPrice} - Close Price: {current.Price_Close}");
                            isBrought = false;
                        }
                    }
                    else
                    {
                        if (buyTime < current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault())
                        {
                            if (current.EMA_5_Crossed_EMA_10_From_Below.GetValueOrDefault() && Converters.GetPips(current.EMA_5, current.EMA_10) >= 1)
                            {
                                bool tradable = true;
                                //if(current.Price_Close > current.EMA_50)
                                //    tradable = true;

                                //if(current.EMA_5_Crossed_EMA_50_From_Below.GetValueOrDefault() && current.EMA_10_Crossed_EMA_50_From_Below.GetValueOrDefault() &&
                                //   current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() < current.EMA_5_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault() &&
                                //   current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() < current.EMA_10_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault())
                                //{
                                //    tradable = true;
                                //}
                                //else
                                //{
                                //    tradable = false;                                    
                                //    if (current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() > current.EMA_5_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault() ||
                                //        current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() > current.EMA_10_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault())
                                //    {
                                //        if (current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() > current.EMA_5_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault())
                                //        {
                                //            var diff = Converters.GetPips(current.EMA_5, current.EMA_50);
                                //            if (diff > 0 && diff < 1)
                                //                tradable = true;
                                //            else
                                //                tradable = false;
                                //        }

                                //        if (current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault() > current.EMA_10_Crossed_EMA_50_From_Above_Dt.GetValueOrDefault())
                                //        {
                                //            var diff = Converters.GetPips(current.EMA_10, current.EMA_50);
                                //            if (diff > 0 && diff < 1) tradable = true;
                                //            else
                                //                tradable = false;
                                //        }
                                //    }
                                //}

                                if (tradable)
                                {
                                    buyTime = current.CurrencyDateTimeEST;
                                    buyPrice = current.Ask_Close;
                                    trailingStopPrice = current.Price_Close - 15 * pip;
                                    isBrought = true;
                                    log.LogInformation($"{current.CurrencyDateTimeEST} - {instrument_name} Trade Open - Current Close:Ask:Bid = {current.Price_Close}:{current.Ask_Close}:{current.Bid_Close}, Trailing Stop Price = {trailingStopPrice}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"BackTestForexTradingFunction failed with error : {ex.ToString()}");
            }
            return new OkObjectResult("done");
        }
    }
}

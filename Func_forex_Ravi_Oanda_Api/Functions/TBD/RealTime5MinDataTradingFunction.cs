//using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
//using Func_forex_Ravi_Oanda_Api.Maps;
//using Func_forex_Ravi_Oanda_Api.Models;
//using Func_forex_Ravi_Oanda_Api.Services;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Func_forex_Ravi_Oanda_Api.Functions
//{
//    public class RealTime5MinDataTradingFunction
//    {
//        private readonly ILogger<RealTime5MinDataTradingFunction> log;
//        private readonly IOandaApi oandaApi;
//        private readonly FxCurrencyTableHelpers table_5min_data;

//        public RealTime5MinDataTradingFunction(ILogger<RealTime5MinDataTradingFunction> log, IOandaApi oandaApi)
//        {
//            this.log = log;
//            this.oandaApi = oandaApi;
//            table_5min_data = new FxCurrencyTableHelpers("oandaforexfivemindatademo");
//        }

//        [FunctionName("RealTime5MinDataTradingFunction")]
//        public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer, ILogger log)
//        {
//            try
//            {
//                log.LogInformation($"RealTime5MinDataTradingFunction function executed at: {DateTime.Now}");

//                // Get UTC Time
//                var timeUtc = DateTime.UtcNow;
//                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
//                DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

//                var accountDetail = await oandaApi.GetAccount();
//                var latesPrices = await oandaApi.GetLatestPrice5Min(Constants.EUR_USD);

//                foreach (var instrument in latesPrices.LatestCandles)
//                {
//                    var get_previous_rows = table_5min_data.GetPreviousEntities(instrument.Instrument, 50);
//                    var latestData = instrument.TransformDataToFxCurrentTable(accountDetail.account, get_previous_rows);

//                    var latest_current = latestData.OrderByDescending(o => o.CurrencyDateTimeUTC).FirstOrDefault();

//                    if (accountDetail.account.openTradeCount > 0)
//                    {
//                        var open_trade_instrument_name = accountDetail.account.trades[0].instrument;
//                        if (accountDetail.account.trades[0].instrument == latest_current.Instrument)
//                        {
//                            if (latest_current.EMA_12_Crossed_EMA_26_From_Above.Value || easternTime.Hour == 7)
//                            {
//                                var tradeCloseRequest = new TradeCloseRequest
//                                {
//                                    units = "ALL"
//                                };
//                                var tradeClosed = await oandaApi.PutCloseTradeRequest(tradeCloseRequest, accountDetail.account.trades[0].id);
//                                if (tradeClosed)
//                                {
//                                    log.LogInformation($"{instrument.Instrument} trade closed");
//                                }
//                                else
//                                    log.LogError($"{instrument.Instrument} trade close failed");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        if (latest_current.EMA_12_Crossed_EMA_26_From_Below.GetValueOrDefault() &&
//                            latest_current.MACD_EMA12_EMA16_DIFF > 0 && easternTime.Hour != 7)
//                        {
//                            bool skipMarketOrder = false;
//                            var filledMarketOrders = await oandaApi.GetFilledMarketOrders(instrument.Instrument);
//                            if (filledMarketOrders != null && filledMarketOrders.orders != null && filledMarketOrders.orders.Any())
//                            {
//                                if (filledMarketOrders.orders.Any(o => o.createTime > latest_current.EMA_12_Crossed_EMA_26_From_Below_Dt.GetValueOrDefault()))
//                                {
//                                    skipMarketOrder = true;
//                                }
//                            }

//                            if (!skipMarketOrder)
//                            {
//                                decimal pip = 0.0001m;
//                                var orderRequest = new MarketOrderRequest
//                                {
//                                    order = new MarketOrder
//                                    {
//                                        type = "MARKET",
//                                        instrument = latest_current.Instrument,
//                                        units = Convert.ToInt32(decimal.Parse(accountDetail.account.balance) * latest_current.Leverage / latest_current.Ask_Close),
//                                        stopLossOnFill = new StopLossOnFill
//                                        {
//                                            price = (latest_current.Ask_Close - pip * 10).ToString()
//                                        }
//                                    }
//                                };
//                                var orderPlaced = await oandaApi.PostMarketOrderRequest(orderRequest);
//                                if (orderPlaced != null)
//                                {
//                                    if (!string.IsNullOrEmpty(orderPlaced.orderFillTransaction?.id))
//                                    {
//                                        log.LogInformation($"{instrument.Instrument} order placed, Trade ID: {orderPlaced.orderFillTransaction?.id}");
//                                        var trailingStopOrderRequest = new TrailingStopLossRequest
//                                        {
//                                            order = new TrailingStopLossRequestOrder
//                                            {
//                                                type = "TRAILING_STOP_LOSS",
//                                                tradeID = orderPlaced.orderFillTransaction?.id,
//                                                distance = (pip * 5).ToString(),
//                                                timeInForce = "GTC"
//                                            }
//                                        };
//                                        var trailingStoporderPlaced = await oandaApi.PostTrailingStopLossRequest(trailingStopOrderRequest);
//                                        if(trailingStoporderPlaced)
//                                            log.LogInformation($"{instrument} trailing stop order placed");
//                                    }
//                                    else
//                                    {
//                                            log.LogError($"{instrument.Instrument} order fill failed");
//                                    }
//                                }
//                                else
//                                    log.LogError($"{instrument.Instrument} order place failed");
//                            }
//                            else
//                                log.LogError($"{instrument.Instrument} order place skipped as order was already placed earlier and closed");

//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                log.LogError($"RealTime5MinDataTradingFunction failed with error : {ex.ToString()}");
//            }
//        }
//    }
//}
//using Func_forex_Ravi_Oanda_Api.Currencies.Helpers;
//using Func_forex_Ravi_Oanda_Api.Models;
//using Func_forex_Ravi_Oanda_Api.Services.Impl;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//namespace Func_forex_Ravi_Oanda_Api.Currencies.Impl
//{
//    public class Instrument
//    {
//        private readonly ILogger log;
//        string instrument_name = "EUR_USD";
//        decimal pip = 0.0001m;
//        public decimal ema_5_previous, ema_5_current, ema_20_previous, ema_20_current,ema_50_previous, ema_50_current;
//        private OandaApi oandaApi {get;set;}
//        private bool ema5_crossed_ema20_from_above {get;set;}
//        private bool ema5_crossed_ema20_from_below {get;set;}
//        private bool ema20_crossed_ema50_from_above {get;set;}
//        private bool ema20_crossed_ema50_from_below {get;set;}
//        private bool ema5_crossed_ema50_from_above {get;set;}
//        private bool ema5_crossed_ema50_from_below {get;set;}
//        public Instrument(ILogger log, string instrument_name, OandaApi oandaApi)
//        {
//            this.log = log;
//            this.instrument_name = instrument_name;
//            this.oandaApi = oandaApi;
//        }
//        public async Task<PricingModel> GetLatestPricingWithHistory()
//        {
//            var pricingHistory = await oandaApi.GetPriceHistory(instrument_name);
//            pricingHistory.Candles = pricingHistory.Candles.OrderBy(o => o.Time).ToList();  
//            // Remove Latest Candle from History
//            pricingHistory.Candles.RemoveAt(pricingHistory.Candles.Count - 1);
//            var latestPrice = await oandaApi.GetLatestPrice(instrument_name);
//            pricingHistory.Candles.Add(latestPrice.LatestCandles.Where(o => o.Instrument == instrument_name).FirstOrDefault().Candles.OrderByDescending(o => o.Time).FirstOrDefault());
//            Calculate_EMA(pricingHistory);   
//            return pricingHistory;
//        }

//        public void Calculate_EMA(PricingModel pricingHistory)
//        {
//            (ema_5_previous, ema_5_current) = formulas.GetEMA(5, pricingHistory.Candles);
//            (ema_20_previous, ema_20_current) = formulas.GetEMA(20, pricingHistory.Candles);
//            (ema_50_previous, ema_50_current) = formulas.GetEMA(50, pricingHistory.Candles);  

//            if(ema_5_current > ema_20_current && ema_5_previous < ema_20_previous)
//            {
//                ema5_crossed_ema20_from_below = true;
//                ema5_crossed_ema20_from_above = false;
//            }

//            if(ema_5_current < ema_20_current && ema_5_previous > ema_20_previous)
//            {
//                ema5_crossed_ema20_from_below = false;
//                ema5_crossed_ema20_from_above = true;
//            }  

//            if(ema_20_current > ema_50_current && ema_20_previous < ema_50_previous)
//            {
//                ema20_crossed_ema50_from_below = true;
//                ema20_crossed_ema50_from_above = false;
//            }

//            if(ema_20_current < ema_50_current && ema_20_previous > ema_50_previous)
//            {
//                ema20_crossed_ema50_from_below = false;
//                ema20_crossed_ema50_from_above = true;
//            }   

//            if(ema_5_current > ema_50_current && ema_5_previous < ema_50_previous)
//            {
//                ema20_crossed_ema50_from_below = true;
//                ema20_crossed_ema50_from_above = false;
//            }

//            if(ema_5_current < ema_50_current && ema_5_previous > ema_50_previous)
//            {
//                ema5_crossed_ema50_from_below = false;
//                ema5_crossed_ema50_from_above = true;
//            }          
//        }

//        public async Task<AccountModel> GetLatestPrice_buy_sell(AccountModel accountDetail)
//        {
//            if(accountDetail.account.openTradeCount > 0)
//            {
//                if(accountDetail.account.trades[0].instrument != instrument_name)
//                    return accountDetail;
//            }

//            var pricingHistory = await GetLatestPricingWithHistory();
//            var latestCandle = pricingHistory.Candles.OrderByDescending(o => o.Time).FirstOrDefault();             

//            if(accountDetail.account.openTradeCount > 0)
//            {
//                //ema20_crossed_ema50_from_above = true;
//                if (ema5_crossed_ema20_from_above || ema5_crossed_ema50_from_above)
//                {
//                    var tradeCloseRequest = new TradeCloseRequest{
//                        units = "ALL"
//                    };                    
//                    var tradeClosed = await oandaApi.PutCloseTradeRequest(tradeCloseRequest, accountDetail.account.trades[0].id);
//                    if(tradeClosed)
//                    {
//                        return await oandaApi.GetAccount();
//                    }
//                }
//            }
//            else
//            {
//                // ema5_crossed_ema20_from_below = true;
//                // ema20_crossed_ema50_from_below = true;
//                // ema5_crossed_ema50_from_below = true;
//                var leverage = Convert.ToInt32(1 / decimal.Parse(accountDetail.account.marginRate));
//                if(ema5_crossed_ema20_from_below)
//                {
//                    var orderRequest = new OrderRequest{
//                        order = new MarketOrder
//                        {
//                            type = "MARKET",
//                            instrument = instrument_name,
//                            units = Convert.ToInt32((decimal.Parse(accountDetail.account.balance) * leverage) /latestCandle.Ask.O),
//                            stopLossOnFill = new StopLossOnFill{
//                                price = (latestCandle.Ask.O - (pip * 10)).ToString()
//                            }
//                        }
//                    };                     

//                    var orderPlaced = await oandaApi.PostOrderRequest(orderRequest);
//                    if(orderPlaced)
//                        log.LogInformation($"{System.DateTime.Now}: {instrument_name}: Trade Placed at Bid({latestCandle.Bid.O}):Close{latestCandle.Mid.C}:Ask{latestCandle.Ask.O}, Units {orderRequest.order.units}");
//                    else
//                        log.LogInformation($"{System.DateTime.Now}: {instrument_name}: Trade Place Failed at Bid({latestCandle.Bid.O}):Close{latestCandle.Mid.C}:Ask{latestCandle.Ask.O}, Units {orderRequest.order.units}");
                        
//                    return await oandaApi.GetAccount();
//                }               
//            }
//            log.LogInformation($"{System.DateTime.Now}: {instrument_name}: Trade Skipped, EMA5/20/50 Previous:{ema_5_previous}/{ema_20_previous}/{ema_50_previous} Current: {ema_5_current}/{ema_20_current}/{ema_50_current}");
//            return accountDetail;
//        }
//    }
//}
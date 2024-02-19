using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Maps;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic
{
    public class TradeInstrument
    {
        private readonly ILogger<TradeInstrument> log;
        private readonly IOandaApi oandaApi;
        private readonly FxTradeTableHelpers fxTradeTableHelpers;
        decimal pip = 0.0001m;
        public TradeInstrument(ILogger<TradeInstrument> log, IOandaApi oandaApi, FxTradeTableHelpers fxTradeTableHelpers)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            this.fxTradeTableHelpers = fxTradeTableHelpers;
        }
        public async Task Run(string instrument_name, List<FxCurrencyTable> fxdata)
        {
            try
            {
                Account account = null;
                string marginRate = null;
                var accountDetail = await oandaApi.GetAccount();
                if (accountDetail != null)
                    account = accountDetail.account;

                var accountInstrumentDetail = await oandaApi.GetAccountInstrumentDetails(instrument_name);
                if (accountInstrumentDetail != null && accountInstrumentDetail.instruments.Count > 0)
                    marginRate = accountInstrumentDetail.instruments[0].marginRate;

                if (account == null || string.IsNullOrEmpty(marginRate))
                {
                    log.LogError($"{instrument_name} Skipping Trading As Failed To Read Account & Instrument Margin Rate");
                    return;
                }
                                
                fxdata = fxdata.OrderByDescending(o => o.RowKey).ToList();
                fxdata = fxdata.TakeLast(2).ToList();
                var previous = fxdata[0];
                var current = fxdata[1];

                Trade trade = null;

                if (account.openTradeCount > 0)
                {
                    trade = account.trades.Where(o => o.instrument == instrument_name).FirstOrDefault();
                }

                if (trade != null)
                {
                    await fxTradeTableHelpers.UpsertEntityAsync(trade.TransformTradeToFxTradeTable());

                    var current_trade_price = decimal.Parse(trade.price);
                    var profit_trade_price_5_pips = current_trade_price + (pip * 5);
                    var profit_trade_price_10_pips = current_trade_price + (pip * 10);

                    var initial_units = int.Parse(trade.initialUnits);
                    var current_units = int.Parse(trade.currentUnits);
                    var tradable_units = current_units;
                    var stopLossOrderID = trade.stopLossOrderID;
                    
                    
                    if (current.Ask_Close >= profit_trade_price_5_pips && current.Ask_Close < profit_trade_price_10_pips)
                    {
                        if(initial_units == current_units && current_units > 1)
                            await oandaApi.PutCloseTradeRequest(instrument_name, trade.id, (current_units / 2));  // Selling Half

                        if (!string.IsNullOrEmpty(stopLossOrderID))
                        {
                            var old_stoploss_price = decimal.Parse(account.orders.Where(o => o.id == stopLossOrderID).FirstOrDefault().price);
                            var new_stoploss_price = (current_trade_price + current.Ask_Close) / 2;
                            if(new_stoploss_price > old_stoploss_price)
                                await oandaApi.PutStopLossRequest(instrument_name, trade.id, stopLossOrderID, new_stoploss_price);
                        }
                    }                    
                    else if (current.Ask_Close >= profit_trade_price_10_pips)
                        await oandaApi.PutCloseTradeRequest(instrument_name, trade.id);  // Selling All
                    else if (current.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault())
                        await oandaApi.PutCloseTradeRequest(instrument_name, trade.id);
                    //else if (current.RSI_14 >= 70)
                    //    await oandaApi.PutCloseTradeRequest(instrument_name, trade.id);
                    else if (Converters.GetCurrentEasternTime().Hour == 6 && Converters.GetCurrentEasternTime().Minute >= 45 && Converters.GetCurrentEasternTime().Minute <= 59)
                        await oandaApi.PutCloseTradeRequest(instrument_name, trade.id);
                    else
                    {
                        var trailingStopLossOrderID = trade.trailingStopLossOrderID;
                        if (!string.IsNullOrEmpty(trailingStopLossOrderID))
                        {
                            var old_trailing_stoploss_distance = account.orders.Where(o => o.id == trailingStopLossOrderID).FirstOrDefault().distance;
                            if (!string.IsNullOrEmpty(old_trailing_stoploss_distance))
                            {
                                decimal stoploss_price = (current.EMA_10 - pip * 5);
                                decimal new_trailing_stoploss_distance = Math.Round(current.Price_Close - stoploss_price, 4);

                                if (new_trailing_stoploss_distance < decimal.Parse(old_trailing_stoploss_distance) && new_trailing_stoploss_distance >= pip * 5)
                                    await oandaApi.PutTrailingStopLossRequest(instrument_name, trade.id, trailingStopLossOrderID, new_trailing_stoploss_distance);
                            }
                        }
                    }
                }
                else
                {
                    if (current.EMA_5_Crossed_EMA_10_From_Below.GetValueOrDefault() && 
                        (Converters.GetPips(current.EMA_5, current.EMA_10) >= 0.5m || 
                        (Converters.GetPips(previous.EMA_5, previous.EMA_10) >= 0.1m)))
                    {
                        var prevTradeList = fxTradeTableHelpers.GetPreviousEntities(instrument_name, 20);

                        log.LogInformation($"EMA_5: {current.EMA_5}, EMA_10: {current.EMA_10}, EMA5_10_Spread: {Converters.GetPips(current.EMA_5, current.EMA_10)}, EMA_5_Crossed_EMA_10_From_Below_Dt : {current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault()}");
                        bool skipMarketOrder = false;
                        //var filledMarketOrders = await oandaApi.GetFilledMarketOrders(instrument_name);



                        if (Converters.GetCurrentEasternTime().Hour == 7)
                        {
                            log.LogInformation("Skip Order It is Eastern Hour 7. High chance of market to crash when market opens at eastern hour");
                            skipMarketOrder = true;
                        }

                        if (Converters.GetCurrentEasternTime().Hour == 6 && Converters.GetCurrentEasternTime().Minute >= 30 && Converters.GetCurrentEasternTime().Minute <= 59)
                        {
                            log.LogInformation("Skip Order It is Eastern Hour 6:30 - 6:59. High chance of market to crash when market opens at eastern hour");
                            skipMarketOrder = true;
                        }

                        //if (filledMarketOrders != null && filledMarketOrders.orders != null && filledMarketOrders.orders.Any())
                        //{
                        //    if (filledMarketOrders.orders.Any(o => o.createTime > current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault()))
                        //    {
                        //        skipMarketOrder = true;
                        //        log.LogInformation($"{instrument_name} Skipping Market Order (Second Market Order In Same EMA Cross)");
                        //    }
                        //}


                        if (prevTradeList != null && prevTradeList.Any())
                        {
                            log.LogInformation($"Previous Trade Open Time UTC: {DateTime.Parse(prevTradeList[0].openTimeUTC)}");
                            if (prevTradeList.Any(o => DateTimeOffset.Parse(o.openTimeUTC).DateTime >= current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault().DateTime))
                            {
                                skipMarketOrder = true;
                                log.LogInformation($"{instrument_name} Skipping Market Order (Second Market Order In Same EMA Cross)");
                            }
                        }

                        if ((current.CurrencyDateTimeUTC - current.EMA_5_Crossed_EMA_10_From_Below_Dt).Value.Minutes > 60)
                        { 
                            skipMarketOrder = true;
                            log.LogInformation("Skip Order EMA 5 - 10 Cross Occured More than 60 min ago");
                        }

                        if (!skipMarketOrder && decimal.Parse(account.marginAvailable) > 0)
                        {
                            var stoploss_price = Math.Round((current.EMA_10 - pip * 5), 5);
                            var trailing_stoploss_distance = Math.Round(current.Price_Close - stoploss_price, 4);

                            var tradeId = await oandaApi.PostMarketOrderRequest(instrument_name, decimal.Parse(account.marginAvailable), current.Leverage.Value, current.Ask_Close, current.Price_Close, stoploss_price);
                            if(!string.IsNullOrEmpty(tradeId) && trailing_stoploss_distance >= pip * 5)
                                await oandaApi.PostTrailingStopLossRequest(instrument_name, tradeId, trailing_stoploss_distance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{instrument_name} Failed To Put Trade Order, Exception {ex.ToString()}");
            }
        }
    }
}

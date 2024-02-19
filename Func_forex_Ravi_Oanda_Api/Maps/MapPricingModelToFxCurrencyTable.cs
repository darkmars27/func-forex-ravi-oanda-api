using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Maps
{
    public static class MapPricingModelToFxCurrencyTable
    {
        public static List<FxCurrencyTable> TransformPricingModelToFxCurrencyTable(this PricingModel priceHistory, AccountInstrumentDetail instrumentDetail)
        {
            if (priceHistory != null && priceHistory.Candles != null && priceHistory.Candles.Any())
            {
                priceHistory.Candles = priceHistory.Candles.OrderBy(o => o.Time).ToList();
                var result = priceHistory.Candles.Select(candle => new FxCurrencyTable
                {
                    PartitionKey = priceHistory.Instrument,
                    RowKey = (DateTime.MaxValue.Ticks - candle.Time.Ticks).ToString(),
                    Instrument = priceHistory.Instrument,
                    Granularity = priceHistory.Granularity,
                    CurrencyDateTimeUTC = candle.Time.ToUniversalTime(),
                    CurrencyDateTimeEST = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(candle.Time.ToUniversalTime(), "Eastern Standard Time"),
                    Margin = instrumentDetail.instruments[0].marginRate,
                    Leverage = Convert.ToInt32(1 / decimal.Parse(instrumentDetail.instruments[0].marginRate)),
                    Bid_Open = candle.Bid.O,
                    Bid_Close = candle.Bid.C,
                    Price_Open = candle.Mid.O,
                    Price_Close = candle.Mid.C,
                    Price_High = candle.Mid.H,
                    Price_Low = candle.Mid.L,
                    Ask_Open = candle.Ask.O,
                    Ask_Close = candle.Ask.C,
                    Spread = ((candle.Ask.C - candle.Bid.C) * 10000)
                }).ToList();
                return result;
            }
            else
                return null;
        }

        public static List<FxCurrencyTable> TransformPricingLatestModelToFxCurrencyTable(this PricingLatestModel priceLatest, AccountInstrumentDetail instrumentDetail)
        {
            if (priceLatest != null && priceLatest.LatestCandles != null && priceLatest.LatestCandles.Any() && priceLatest.LatestCandles[0].Candles != null && priceLatest.LatestCandles[0].Candles.Any())
            {
                priceLatest.LatestCandles[0].Candles = priceLatest.LatestCandles[0].Candles.OrderBy(o => o.Time).ToList();
                var result = priceLatest.LatestCandles[0].Candles.Select(candle => new FxCurrencyTable
                {
                    PartitionKey = priceLatest.LatestCandles[0].Instrument,
                    RowKey = (DateTime.MaxValue.Ticks - candle.Time.Ticks).ToString(),
                    Instrument = priceLatest.LatestCandles[0].Instrument,
                    Granularity = priceLatest.LatestCandles[0].Granularity,
                    CurrencyDateTimeUTC = candle.Time.ToUniversalTime(),
                    CurrencyDateTimeEST = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(candle.Time.ToUniversalTime(), "Eastern Standard Time"),
                    Margin = instrumentDetail.instruments[0].marginRate,
                    Leverage = Convert.ToInt32(1 / decimal.Parse(instrumentDetail.instruments[0].marginRate)),
                    Bid_Open = candle.Bid.O,
                    Bid_Close = candle.Bid.C,
                    Price_Open = candle.Mid.O,
                    Price_Close = candle.Mid.C,
                    Price_High = candle.Mid.H,
                    Price_Low = candle.Mid.L,
                    Ask_Open = candle.Ask.O,
                    Ask_Close = candle.Ask.C,
                    Spread = ((candle.Ask.C - candle.Bid.C) * 10000)
                }).ToList();
                return result;
            }
            else
                return null;
        }

        public static FxTradeTable TransformTradeToFxTradeTable(this Trade trade)
        {
            if(trade != null)
            {
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                var openTime = DateTime.Parse(trade.openTime).ToUniversalTime();
                openTime = DateTime.SpecifyKind(openTime, DateTimeKind.Utc);
                var result = new FxTradeTable
                {
                    PartitionKey = trade.instrument,
                    RowKey = (DateTime.MaxValue.Ticks - openTime.Ticks).ToString(),
                    Id = trade.id,
                    currentUnits = trade.currentUnits,
                    initialMarginRequired = trade.initialMarginRequired,
                    initialUnits = trade.initialUnits,
                    Instrument = trade.instrument,
                    marginUsed = trade.marginUsed,
                    openTimeUTC = trade.openTime,
                    openTimeEST = TimeZoneInfo.ConvertTimeFromUtc(openTime, easternZone).ToString(),
                    Price = trade.price,
                    state = trade.state,
                    stopLossOrderID = trade.stopLossOrderID,
                    trailingStopLossOrderID = trade.trailingStopLossOrderID,
                    unrealizedPL = trade.unrealizedPL
                };

                return result;
            }
            return null;
        }
    }
}

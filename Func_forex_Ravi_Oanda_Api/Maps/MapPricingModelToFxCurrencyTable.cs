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
                priceHistory.Candles = priceHistory.Candles.Where(o => o.Complete).OrderBy(o => o.Time).ToList();
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
                priceLatest.LatestCandles[0].Candles = priceLatest.LatestCandles[0].Candles.Where(o => o.Complete).OrderBy(o => o.Time).ToList();
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
    }
}

using Func_forex_Ravi_Oanda_Api.Currencies.Helpers;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using Polly.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Maps
{
    public static class Map_Hist_Data_To_FxCurrencyTable
    {
        public static List<FxCurrencyTable> TransformHistDataToFxCurrentTable(this PricingModel priceHistory, Account account)
        {
            priceHistory.Candles = priceHistory.Candles.OrderBy(o => o.Time).ToList();
            var response = new List<FxCurrencyTable>();

            var processedCandles = new List<Candle>();
            foreach(var candle in priceHistory.Candles)
            {
                processedCandles.Add(candle);
                var row = new FxCurrencyTable
                {
                    PartitionKey = priceHistory.Instrument,
                    RowKey = (DateTime.MaxValue.Ticks - candle.Time.Ticks).ToString(),
                    Instrument = priceHistory.Instrument,
                    Granularity = priceHistory.Granularity,
                    CurrencyDateTimeUTC = candle.Time.ToUniversalTime(),
                    CurrencyDateTimeEST = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(candle.Time.ToUniversalTime(), "Eastern Standard Time"),
                    Cash = account.balance,
                    Margin = account.marginRate,
                    Leverage = Convert.ToInt32(1 / decimal.Parse(account.marginRate)),
                    Bid_Open = candle.Bid.O,
                    Bid_Close = candle.Bid.C,
                    Price_Open = candle.Mid.O,
                    Price_Close = candle.Mid.C,
                    Price_High = candle.Mid.H,
                    Price_Low = candle.Mid.L,
                    Ask_Open = candle.Ask.O,
                    Ask_Close = candle.Ask.C,
                    Spread = ((candle.Ask.C - candle.Bid.C) * 10000),
                    SMA_5 = Formulas.GetSMA(processedCandles, 5),
                    SMA_10 = Formulas.GetSMA(processedCandles, 10),
                    SMA_20 = Formulas.GetSMA(processedCandles, 20),
                    SMA_50 = Formulas.GetSMA(processedCandles, 50),
                };
                response.Add(row);
            }

            // Indicators require minimum two days data so starting the loop from 2nd row
            for(int i=1; i< response.Count; i++)
            {
                var currentRow = response[i];
                var previousRow = response[i - 1];
                response[i].EMA_5 = Formulas.GetEMA(previousRow.EMA_5 > 0 ? previousRow.EMA_5 : previousRow.SMA_5, currentRow.Price_Close, 5);
                response[i].EMA_10 = Formulas.GetEMA(previousRow.EMA_10 > 0 ? previousRow.EMA_10 : previousRow.SMA_10, currentRow.Price_Close, 10);
                response[i].EMA_20 = Formulas.GetEMA(previousRow.EMA_20 > 0 ? previousRow.EMA_20 : previousRow.SMA_20, currentRow.Price_Close, 20);
                response[i].EMA_50 = Formulas.GetEMA(previousRow.EMA_50 > 0 ? previousRow.EMA_50 : previousRow.SMA_50, currentRow.Price_Close, 50);

                response[i].EMA_Diff_5_20_pips = ((response[i].EMA_5 - response[i].EMA_20) * 10000);
                response[i].EMA_Diff_5_50_pips = ((response[i].EMA_5 - response[i].EMA_50) * 10000);
                response[i].EMA_Diff_20_50_pips = ((response[i].EMA_20 - response[i].EMA_50) * 10000);

                if (previousRow.Avg_Current_Gain_14 == 0 || previousRow.Avg_Current_Loss_14 == 0)
                    (response[i].Avg_Current_Gain_14, response[i].Avg_Current_Loss_14) = Formulas.GetRSI_AverageGainorLoss_14DayAvg(response.Take(i+1).ToList(), 14);
                else
                    (response[i].RSI_14, response[i].Avg_Current_Gain_14, response[i].Avg_Current_Loss_14) = Formulas.GetRSI_AverageGainorLoss_PreviousRSI(currentRow, previousRow, 14);
            }
            return response;
        }
    }
}

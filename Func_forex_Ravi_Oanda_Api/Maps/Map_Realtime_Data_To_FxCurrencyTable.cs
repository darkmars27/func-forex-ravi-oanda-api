using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Currencies.Helpers;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Maps
{
    public static class Map_Realtime_Data_To_FxCurrencyTable
    {
        public static List<FxCurrencyTable> TransformQuarterHourDataToFxCurrentTable(this PricingLatestModel latestPrice, Account account)
        {
            var response = new List<FxCurrencyTable>();
            foreach (var instrumentCandles in latestPrice.LatestCandles)
            {
                foreach(var candle in instrumentCandles.Candles)
                {
                    var row = new FxCurrencyTable
                    {
                        PartitionKey = instrumentCandles.Instrument,
                        RowKey = candle.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                        Instrument = instrumentCandles.Instrument,
                        Granularity = instrumentCandles.Granularity,
                        CurrencyDateTimeUTC = candle.Time.ToUniversalTime(),
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
                        Ask_Close = candle.Ask.C
                    };
                    response.Add(row);
                }
            }
            return response;
        }

        public static (decimal, decimal, decimal) CalculateAlgorithms(this FxCurrencyTable currentRow, List<FxCurrencyTable> previous_14_rows)
        {
            var previousRow = previous_14_rows.LastOrDefault();
            currentRow.EMA_5 = Formulas.GetEMA(previousRow.EMA_5, currentRow.Price_Close, 5);
            currentRow.EMA_10 = Formulas.GetEMA(previousRow.EMA_10, currentRow.Price_Close, 10);
            currentRow.EMA_20 = Formulas.GetEMA(previousRow.EMA_20, currentRow.Price_Close, 20);
            currentRow.EMA_50 = Formulas.GetEMA(previousRow.EMA_50, currentRow.Price_Close, 50);

            previous_14_rows.Add(currentRow);
            (currentRow.RSI_14, currentRow.Avg_Current_Gain_14, currentRow.Avg_Current_Loss_14) = Formulas.GetRSI_AverageGainorLoss_PreviousRSI(previous_14_rows, previousRow, 14);
            return currentRow;
        }
    }
}

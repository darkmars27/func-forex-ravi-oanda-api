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
        public static List<FxCurrencyTable> TransformDataToFxCurrentTable(this PricingModel instrumentlatestPrices, Account account, List<FxCurrencyTable> historicalPrices)
        {
            historicalPrices = historicalPrices.OrderByDescending(o => o.RowKey).ToList();
            var response = new List<FxCurrencyTable>();

            foreach (var candle in instrumentlatestPrices.Candles)
            {
                var row = new FxCurrencyTable
                {
                    PartitionKey = instrumentlatestPrices.Instrument,
                    RowKey = (DateTime.MaxValue.Ticks - candle.Time.Ticks).ToString(),
                    Instrument = instrumentlatestPrices.Instrument,
                    Granularity = instrumentlatestPrices.Granularity,
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
                    Spread = ((candle.Ask.C - candle.Bid.C) * 10000)
                };
                var ex_rk_index = historicalPrices.FindIndex(histrow => histrow.RowKey == row.RowKey);
                if(ex_rk_index != -1)
                {
                    historicalPrices[ex_rk_index] = row;
                }
                else
                {
                    historicalPrices.Add(row);
                    ex_rk_index = historicalPrices.Count - 1;
                }

                // Calculate SMA
                historicalPrices[ex_rk_index].SMA_5 = Formulas.GetSMA(historicalPrices, 5);
                historicalPrices[ex_rk_index].SMA_10 = Formulas.GetSMA(historicalPrices, 10);
                historicalPrices[ex_rk_index].SMA_12 = Formulas.GetSMA(historicalPrices, 12);
                historicalPrices[ex_rk_index].SMA_20 = Formulas.GetSMA(historicalPrices, 20);
                historicalPrices[ex_rk_index].SMA_26 = Formulas.GetSMA(historicalPrices, 26);
                historicalPrices[ex_rk_index].SMA_50 = Formulas.GetSMA(historicalPrices, 50);

                var currentRow = historicalPrices[ex_rk_index];
                var previousRow = historicalPrices[ex_rk_index - 1];
                historicalPrices[ex_rk_index].EMA_5 = Formulas.GetEMA(previousRow.EMA_5, currentRow.Price_Close, 5);
                historicalPrices[ex_rk_index].EMA_10 = Formulas.GetEMA(previousRow.EMA_10, currentRow.Price_Close, 10);
                historicalPrices[ex_rk_index].EMA_12 = Formulas.GetEMA(previousRow.EMA_12, currentRow.Price_Close, 12);
                historicalPrices[ex_rk_index].EMA_20 = Formulas.GetEMA(previousRow.EMA_20, currentRow.Price_Close, 20);
                historicalPrices[ex_rk_index].EMA_26 = Formulas.GetEMA(previousRow.EMA_26, currentRow.Price_Close, 26);
                historicalPrices[ex_rk_index].EMA_50 = Formulas.GetEMA(previousRow.EMA_50, currentRow.Price_Close, 50);

                historicalPrices[ex_rk_index].EMA_Diff_5_20_pips = ((historicalPrices[ex_rk_index].EMA_5 - historicalPrices[ex_rk_index].EMA_20) * 10000);
                historicalPrices[ex_rk_index].EMA_Diff_5_50_pips = ((historicalPrices[ex_rk_index].EMA_5 - historicalPrices[ex_rk_index].EMA_50) * 10000);
                historicalPrices[ex_rk_index].EMA_Diff_20_50_pips = ((historicalPrices[ex_rk_index].EMA_20 - historicalPrices[ex_rk_index].EMA_50) * 10000);
                historicalPrices[ex_rk_index].MACD_EMA12_EMA16_DIFF = historicalPrices[ex_rk_index].EMA_12 - historicalPrices[ex_rk_index].EMA_26;

                (historicalPrices[ex_rk_index].RSI_14, historicalPrices[ex_rk_index].Avg_Current_Gain_14, historicalPrices[ex_rk_index].Avg_Current_Loss_14) = Formulas.GetRSI_AverageGainorLoss_PreviousRSI(currentRow, previousRow, 14);

                historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Below = historicalPrices[ex_rk_index - 1].EMA_5_Crossed_EMA_20_From_Below;
                historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Above = historicalPrices[ex_rk_index - 1].EMA_5_Crossed_EMA_20_From_Above;
                historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Below = historicalPrices[ex_rk_index - 1].EMA_12_Crossed_EMA_26_From_Below;
                historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Above = historicalPrices[ex_rk_index - 1].EMA_12_Crossed_EMA_26_From_Above;
                historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Below = historicalPrices[ex_rk_index - 1].EMA_20_Crossed_EMA_50_From_Below;
                historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Above = historicalPrices[ex_rk_index - 1].EMA_20_Crossed_EMA_50_From_Above;

                if (currentRow.EMA_5 > currentRow.EMA_20 && previousRow.EMA_5 < previousRow.EMA_20)
                {
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Below = true;
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Below_Dt = candle.Time.ToUniversalTime();
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Above = false;
                }

                if (currentRow.EMA_5 < currentRow.EMA_20 && previousRow.EMA_5 > previousRow.EMA_20)
                {
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Below = false;
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Above = true;
                }

                if (currentRow.EMA_12 > currentRow.EMA_26 && previousRow.EMA_12 < previousRow.EMA_26)
                {
                    historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Below = true;
                    historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Below_Dt = candle.Time.ToUniversalTime();
                    historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Above = false;
                }

                if (currentRow.EMA_12 < currentRow.EMA_26 && previousRow.EMA_12 > previousRow.EMA_26)
                {
                    historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Below = false;
                    historicalPrices[ex_rk_index].EMA_12_Crossed_EMA_26_From_Above = true;
                }

                if (currentRow.EMA_5 < currentRow.EMA_20 && previousRow.EMA_5 > previousRow.EMA_20)
                {
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Below = false;
                    historicalPrices[ex_rk_index].EMA_5_Crossed_EMA_20_From_Above = true;
                }

                if (currentRow.EMA_20 > currentRow.EMA_50 && previousRow.EMA_20 < previousRow.EMA_50)
                {
                    historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Below = true;
                    historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Below_Dt = candle.Time.ToUniversalTime();
                    historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Above = false;
                }

                if (currentRow.EMA_20 < currentRow.EMA_50 && previousRow.EMA_20 > previousRow.EMA_50)
                {
                    historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Below = false;                    
                    historicalPrices[ex_rk_index].EMA_20_Crossed_EMA_50_From_Above = true;
                }

                response.Add(historicalPrices[ex_rk_index]);
            }
            return response;
        }      
    }
}

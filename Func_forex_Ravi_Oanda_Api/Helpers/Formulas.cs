//using Func_forex_Ravi_Oanda_Api.Models;
//using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.WindowsAzure.Storage.Blob.Protocol;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//namespace Func_forex_Ravi_Oanda_Api.Currencies.Helpers
//{
//    public static class Formulas
//    {
//        public (decimal, decimal) GetEMA(int period, List<Candle> candles)
//        {
//            var filteredCandlesSMA = candles.TakeLast(period).ToList();
//            var sma = filteredCandlesSMA.Sum(o => o.Mid.C)/filteredCandlesSMA.Count ;
//            var filteredCandles = candles.Skip(period).ToList();

//            decimal previousEma = sma;
//            decimal currentEma = 0;
//            var multiplier = (decimal)2/ (1 + period);
//            bool isFirstRecord = true;
//            foreach(var candle in filteredCandles)
//            {          
//                if(!isFirstRecord)             
//                    previousEma = currentEma;

//                //Console.WriteLine(candle.Mid.C);
//                currentEma = (candle.Mid.C * multiplier) + (previousEma * (1 - multiplier));                
//                isFirstRecord = false;
//            }
//            return (previousEma, currentEma);
//        }

//        public static decimal GetSMA(List<Candle> candles, int period)
//        {
//            if (candles.Count >= period)
//            {
//                var filteredCandlesSMA = candles.TakeLast(period).ToList();
//                var sma = filteredCandlesSMA.Sum(o => o.Mid.C) / filteredCandlesSMA.Count;
//                return sma;
//            }
//            else
//                return 0;
//        }

//        public static decimal GetSMA(List<FxCurrencyTable> rows, int period)
//        {
//            if (rows.Count >= period)
//            {
//                var filteredCandlesSMA = rows.TakeLast(period).ToList();
//                var sma = filteredCandlesSMA.Sum(o => o.Price_Close) / filteredCandlesSMA.Count;
//                return sma;
//            }
//            else
//                return 0;
//        }

//        public static decimal GetEMA(decimal previousEma, decimal current_close_price, int period)
//        {
//            if (previousEma > 0)
//            {
//                var multiplier = (decimal)2 / (1 + period);
//                var currentEma = (current_close_price * multiplier) + (previousEma * (1 - multiplier));
//                return currentEma;
//            }
//            else
//                return 0;
//        }

       

//        //public static (decimal,decimal,decimal) GetRSI_AverageGainorLoss_14DayAvg(List<FxCurrencyTable> rows, int period)
//        //{
//        //    if (rows.Count >= period)
//        //    {
//        //        rows = rows.Take(period).ToList();
//        //        decimal gain = 0.0m;
//        //        decimal loss = 0.0m;

//        //        // Calculate initial average gain and loss
//        //        for (int i = 1; i < rows.Count; i++)
//        //        {
//        //            decimal change = rows[i].Price_Close - rows[i - 1].Price_Close;
//        //            if (change > 0)
//        //            {
//        //                gain += change;
//        //            }
//        //            else
//        //            {
//        //                loss -= change; // Use absolute value of loss
//        //            }
//        //        }

//        //        decimal averageGain = gain / (period);
//        //        decimal averageLoss = loss / (period);
//        //        decimal rsi = 100 - (100 / (1 + (averageGain / averageLoss)));
//        //        return (rsi, averageGain, averageLoss);
//        //    }
//        //    else
//        //        return (0,0,0);
//        //}

        

        
//    }
//}
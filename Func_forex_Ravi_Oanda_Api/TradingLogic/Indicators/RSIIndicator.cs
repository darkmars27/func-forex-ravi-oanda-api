//using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators
//{
//    public static class RSIIndicator
//    {
//        public static (decimal, decimal) GetRSI_AverageGainorLoss_14DayAvg(List<FxCurrencyTable> rows, int period)
//        {
//            if (rows.Count >= period)
//            {
//                rows = rows.TakeLast(period).ToList();
//                decimal gain = 0.0m;
//                decimal loss = 0.0m;

//                // Calculate initial average gain and loss
//                for (int i = 1; i < rows.Count; i++)
//                {
//                    decimal change = rows[i].Price_Close - rows[i - 1].Price_Close;
//                    if (change > 0)
//                    {
//                        gain += change;
//                    }
//                    else
//                    {
//                        loss -= change;
//                    }
//                }

//                decimal averageGain = gain / (period);
//                decimal averageLoss = loss / (period);
//                //decimal rsi = 100 - (100 / (1 + (averageGain / averageLoss)));
//                return (averageGain, averageLoss);
//            }
//            else
//                return (0, 0);
//        }

//        public static (decimal, decimal, decimal) GetRSI_AverageGainorLoss_PreviousRSI(FxCurrencyTable currentCandle, FxCurrencyTable previousCandle, int period)
//        {
//            decimal gain = 0.0m;
//            decimal loss = 0.0m;
//            var change = currentCandle.Price_Close - previousCandle.Price_Close;
//            if (change > 0)
//            {
//                gain += change;
//            }
//            else
//            {
//                loss -= change;
//            }

//            decimal averageGain = ((previousCandle.Avg_Current_Gain_14 * 13) + gain) / period;
//            decimal averageLoss = ((previousCandle.Avg_Current_Loss_14 * 13) + loss) / period;

//            decimal rsi = 100 - (100 / (1 + (averageGain / averageLoss)));
//            return (rsi, averageGain, averageLoss);
//        }
//    }
//}

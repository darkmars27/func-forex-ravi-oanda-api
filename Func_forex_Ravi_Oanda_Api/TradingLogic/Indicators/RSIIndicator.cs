using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators
{
    public static class RSIIndicator
    {
        public static (decimal, decimal) GetRSI_AverageGainorLoss_14DayAvg(List<FxCurrencyTable> fxdata, int index, int period)
        {
            try
            {
                fxdata = fxdata.GetPreviousRange(index, period);
                if (fxdata.Count >= period)
                {
                    decimal gain = 0.0m;
                    decimal loss = 0.0m;

                    // Calculate initial average gain and loss
                    for (int i = 1; i < fxdata.Count; i++)
                    {
                        decimal change = fxdata[i].Price_Close - fxdata[i - 1].Price_Close;
                        if (change > 0)
                        {
                            gain += change;
                        }
                        else
                        {
                            loss -= change;
                        }
                    }

                    decimal averageGain = gain / (period);
                    decimal averageLoss = loss / (period);
                    //decimal rsi = 100 - (100 / (1 + (averageGain / averageLoss)));
                    return (averageGain, averageLoss);
                }
                else
                    return (0, 0);
            }
            catch
            {
                return (0, 0);
            }
        }

        public static (decimal, decimal, decimal) GetRSI_AverageGainorLoss_PreviousRSI(FxCurrencyTable currentCandle, FxCurrencyTable previousCandle, int period)
        {
            decimal gain = 0.0m;
            decimal loss = 0.0m;
            var change = currentCandle.Price_Close - previousCandle.Price_Close;
            if (change > 0)
            {
                gain += change;
            }
            else
            {
                loss -= change;
            }

            decimal averageGain = ((previousCandle.Avg_Current_Gain_14 * 13) + gain) / period;
            decimal averageLoss = ((previousCandle.Avg_Current_Loss_14 * 13) + loss) / period;

            decimal rsi = 100 - (100 / (1 + (averageGain / averageLoss)));
            return (rsi, averageGain, averageLoss);
        }
    }
}

using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators
{
    public static class EMAIndicator
    {
        public static decimal CalculateEMA(this decimal prev_ema, decimal close_price, int period)
        {
            if (prev_ema > 0)
            {
                var multiplier = (decimal)2 / (1 + period);
                var currentEma = (close_price * multiplier) + (prev_ema * (1 - multiplier));
                return currentEma;
            }
            else
                return 0;
        }

        public static FxCurrencyTable DoEMATrendAnalysis(this FxCurrencyTable current, FxCurrencyTable previous)
        {
            current.EMA_5_Crossed_EMA_10_From_Below = previous.EMA_5_Crossed_EMA_10_From_Below;
            current.EMA_5_Crossed_EMA_10_From_Above = previous.EMA_5_Crossed_EMA_10_From_Above;
            current.EMA_10_Crossed_EMA_50_From_Below = previous.EMA_10_Crossed_EMA_50_From_Below;
            current.EMA_10_Crossed_EMA_50_From_Above = previous.EMA_10_Crossed_EMA_50_From_Above;
            current.EMA_5_Crossed_EMA_50_From_Below = previous.EMA_5_Crossed_EMA_50_From_Below;
            current.EMA_5_Crossed_EMA_50_From_Above = previous.EMA_5_Crossed_EMA_50_From_Above;
            current.EMA_5_Crossed_EMA_10_From_Below_Dt = previous.EMA_5_Crossed_EMA_10_From_Below_Dt;
            current.EMA_5_Crossed_EMA_10_From_Above_Dt = previous.EMA_5_Crossed_EMA_10_From_Above_Dt;
            current.EMA_10_Crossed_EMA_50_From_Below_Dt = previous.EMA_10_Crossed_EMA_50_From_Below_Dt;
            current.EMA_10_Crossed_EMA_50_From_Above_Dt = previous.EMA_10_Crossed_EMA_50_From_Above_Dt;
            current.EMA_5_Crossed_EMA_50_From_Below_Dt = previous.EMA_5_Crossed_EMA_50_From_Below_Dt;
            current.EMA_5_Crossed_EMA_50_From_Above_Dt = previous.EMA_5_Crossed_EMA_50_From_Above_Dt;

            // EMA 5 Crossed EMA 10 (Below)
            if (current.EMA_5 > current.EMA_10 && previous.EMA_5 < previous.EMA_10)
            {
                current.EMA_5_Crossed_EMA_10_From_Below = true;
                current.EMA_5_Crossed_EMA_10_From_Below_Dt = current.CurrencyDateTimeEST;
                current.EMA_5_Crossed_EMA_10_From_Above = false;
                current.EMA_5_Crossed_EMA_10_From_Above_Dt = string.Empty.ConvertToDateTimeOffset();
            }
            // EMA 5 Crossed EMA 10 (Above)
            if (current.EMA_5 < current.EMA_10 && previous.EMA_5 > previous.EMA_10)
            {
                current.EMA_5_Crossed_EMA_10_From_Above = true;
                current.EMA_5_Crossed_EMA_10_From_Above_Dt = current.CurrencyDateTimeEST;
                current.EMA_5_Crossed_EMA_10_From_Below = false;
                current.EMA_5_Crossed_EMA_10_From_Below_Dt = string.Empty.ConvertToDateTimeOffset();
            }
            // EMA 10 Crossed EMA 50 (Below)
            if (current.EMA_10 > current.EMA_50 && previous.EMA_10 < previous.EMA_50)
            {
                current.EMA_10_Crossed_EMA_50_From_Below = true;
                current.EMA_10_Crossed_EMA_50_From_Below_Dt = current.CurrencyDateTimeEST;
                current.EMA_10_Crossed_EMA_50_From_Above = false;
                current.EMA_10_Crossed_EMA_50_From_Above_Dt = string.Empty.ConvertToDateTimeOffset();
            }
            // EMA 10 Crossed EMA 50 (Above)
            if (current.EMA_10 < current.EMA_50 && previous.EMA_10 > previous.EMA_50)
            {
                current.EMA_10_Crossed_EMA_50_From_Above = true;
                current.EMA_10_Crossed_EMA_50_From_Above_Dt = current.CurrencyDateTimeEST;
                current.EMA_10_Crossed_EMA_50_From_Below = false;
                current.EMA_10_Crossed_EMA_50_From_Below_Dt = string.Empty.ConvertToDateTimeOffset();
            }
            // EMA 5 Crossed EMA 50 (Below)
            if (current.EMA_5 > current.EMA_50 && previous.EMA_5 < previous.EMA_50)
            {
                current.EMA_5_Crossed_EMA_50_From_Below = true;
                current.EMA_5_Crossed_EMA_50_From_Below_Dt = current.CurrencyDateTimeEST;
                current.EMA_5_Crossed_EMA_50_From_Above = false;
                current.EMA_5_Crossed_EMA_50_From_Above_Dt = string.Empty.ConvertToDateTimeOffset();
            }
            // EMA 5 Crossed EMA 50 (Above)
            if (current.EMA_5 < current.EMA_50 && previous.EMA_5 > previous.EMA_50)
            {
                current.EMA_5_Crossed_EMA_50_From_Above = true;
                current.EMA_5_Crossed_EMA_50_From_Above_Dt = current.CurrencyDateTimeEST;
                current.EMA_5_Crossed_EMA_50_From_Below = false;
                current.EMA_5_Crossed_EMA_50_From_Below_Dt = string.Empty.ConvertToDateTimeOffset();
            }

            return current;
        }
    }
}

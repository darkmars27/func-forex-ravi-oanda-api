using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System.Collections.Generic;
using System.Linq;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators
{
    public class SMAIndicator
    {
        public static decimal CalculateSMA(List<FxCurrencyTable> fxdata, int index, int period)
        {
            try
            {
                fxdata = fxdata.GetPreviousRange(index, period);
                if (fxdata.Count >= period)
                {
                    var sma = fxdata.Sum(o => o.Price_Close) / fxdata.Count;
                    return sma;
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}

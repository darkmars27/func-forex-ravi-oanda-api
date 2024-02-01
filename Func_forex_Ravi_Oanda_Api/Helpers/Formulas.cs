using System.Collections.Generic;
using System.Linq;
namespace Ravi.Oanda.Automation
{
    public class Formulas
    {
        public (decimal, decimal) GetEMA(int period, List<Candle> candles)
        {
            var filteredCandlesSMA = candles.Take(period).ToList();
            var sma = filteredCandlesSMA.Sum(o => o.Mid.C)/filteredCandlesSMA.Count ;
            var filteredCandles = candles.Skip(period).ToList();

            decimal previousEma = sma;
            decimal currentEma = 0;
            var multiplier = (decimal)2/ (1 + period);
            bool isFirstRecord = true;
            foreach(var candle in filteredCandles)
            {          
                if(!isFirstRecord)             
                    previousEma = currentEma;

                //Console.WriteLine(candle.Mid.C);
                currentEma = (candle.Mid.C * multiplier) + (previousEma * (1 - multiplier));                
                isFirstRecord = false;
            }
            return (previousEma, currentEma);
        }
    }
}
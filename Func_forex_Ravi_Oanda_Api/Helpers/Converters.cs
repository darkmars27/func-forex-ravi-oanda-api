using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Helpers
{
    public static class Converters
    {
        public static DateTimeOffset? ConvertToDateTimeOffset(this string input)
        {
            if (!string.IsNullOrEmpty(input))
                return DateTimeOffset.Parse(input);
            else
            {
                var minDateTime = new DateTime(1999, 01, 01, 0, 0, 0);
                minDateTime = DateTime.SpecifyKind(minDateTime, DateTimeKind.Utc);
                DateTimeOffset minDtOffset = minDateTime;
                return minDtOffset;
            }
        }

        public static decimal ConvertToDecimal(this string input)
        {
            if (!string.IsNullOrEmpty(input))
                return decimal.Parse(input);
            else
            {
                return 0;
            }
        }

        public static DateTime GetCurrentEasternTime()
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            return easternTime;
        }

        public static List<FxCurrencyTable> GetPreviousRange(this List<FxCurrencyTable> fxdata, int index, int period)
        {
            int startIndex = index - period;
            int endIndex = period;
            return fxdata.GetRange(startIndex, endIndex).ToList();
        }

        public static string GetValueString(this DateTimeOffset? input)
        {
            if (input.HasValue)
                return input.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            else
            {
                var minDateTime = new DateTime(1999, 01, 01, 0, 0, 0);
                minDateTime = DateTime.SpecifyKind(minDateTime, DateTimeKind.Utc);
                DateTimeOffset minDtOffset = minDateTime;
                return minDtOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
            }
        }

        public static decimal GetPips(decimal price1, decimal price2)
        {
            var difference = (price1 - price2) ;
            return difference * 10000;
        }
    }
}

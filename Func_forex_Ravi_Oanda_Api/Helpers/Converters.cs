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
    }
}

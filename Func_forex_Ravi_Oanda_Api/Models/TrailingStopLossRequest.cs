using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Models
{
    public class TrailingStopLossRequest
    {
        public TrailingStopLossRequestOrder order { get; set; }
    }

    public class TrailingStopLossRequestOrder
    {
        public string type { get; set; }
        public string tradeID { get; set; }
        public string distance { get; set; }
        public string timeInForce { get; set; }
    }
}

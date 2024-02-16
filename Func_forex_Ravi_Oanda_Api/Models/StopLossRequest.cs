using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Models
{
    public class StopLossRequest
    {
        public StopLossRequestOrder order { get; set; }
    }

    public class StopLossRequestOrder
    {
        public string type { get; set; }
        public string tradeID { get; set; }
        public string price { get; set; }
        public string timeInForce { get; set; }
    }
}

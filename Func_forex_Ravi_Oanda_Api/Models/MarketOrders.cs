using System;
using System.Collections.Generic;

namespace Func_forex_Ravi_Oanda_Api.Models
{
    public class MarketOrders
    {
        public List<PastMarketOrder> orders { get; set; }
        public string lastTransactionID { get; set; }
    }

    public class PastMarketOrder
    {
        public string id { get; set; }
        public DateTimeOffset createTime { get; set; }
        public string type { get; set; }
        public string tradeID { get; set; }
        public string price { get; set; }
        public string timeInForce { get; set; }
        public string triggerCondition { get; set; }
        public string triggerMode { get; set; }
        public string state { get; set; }
        public string instrument { get; set; }
        public string units { get; set; }
        public MarketOrderStoplossonfill stopLossOnFill { get; set; }
        public string positionFill { get; set; }
        public string fillingTransactionID { get; set; }
        public string filledTime { get; set; }
        public string tradeOpenedID { get; set; }
        public string[] tradeClosedIDs { get; set; }
        public string cancellingTransactionID { get; set; }
        public string cancelledTime { get; set; }
    }

    public class MarketOrderStoplossonfill
    {
        public string price { get; set; }
        public string timeInForce { get; set; }
        public string triggerMode { get; set; }
    }

}
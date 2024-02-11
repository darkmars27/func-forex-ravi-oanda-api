namespace Func_forex_Ravi_Oanda_Api.Models
{
    public class MarketOrderRequest
    {
        public MarketOrder order {get; set;}
        public class MarketOrder
        {
            public string type { get; set; }
            public string instrument { get; set; }
            public decimal units { get; set; }

            public StopLossOnFill stopLossOnFill { get; set; }
        }

        public class StopLossOnFill
        {
            public string price { get; set; }
        }
    }    
}
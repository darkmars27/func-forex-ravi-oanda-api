using System.Collections.Generic;
namespace Ravi.Oanda.Automation
{
    public class AccountModel
    {
        public Account account { get; set; }
        public string lastTransactionID { get; set; }
    }

    public class Account
    {
        public string guaranteedStopLossOrderMode { get; set; }
        public bool hedgingEnabled { get; set; }
        public string id { get; set; }
        public string createdTime { get; set; }
        public string currency { get; set; }
        public int createdByUserID { get; set; }
        public string alias { get; set; }
        public string marginRate { get; set; }
        public string lastTransactionID { get; set; }
        public string balance { get; set; }
        public int openTradeCount { get; set; }
        public int openPositionCount { get; set; }
        public int pendingOrderCount { get; set; }
        public string pl { get; set; }
        public string resettablePL { get; set; }
        public string resettablePLTime { get; set; }
        public string financing { get; set; }
        public string commission { get; set; }
        public string dividendAdjustment { get; set; }
        public string guaranteedExecutionFees { get; set; }
        public List<Order> orders { get; set; }
        public List<Position> positions { get; set; }
        public List<Trade> trades { get; set; }
        public string unrealizedPL { get; set; }
        public string NAV { get; set; }
        public string marginUsed { get; set; }
        public string marginAvailable { get; set; }
        public string positionValue { get; set; }
        public string marginCloseoutUnrealizedPL { get; set; }
        public string marginCloseoutNAV { get; set; }
        public string marginCloseoutMarginUsed { get; set; }
        public string marginCloseoutPositionValue { get; set; }
        public string marginCloseoutPercent { get; set; }
        public string withdrawalLimit { get; set; }
        public string marginCallMarginUsed { get; set; }
        public string marginCallPercent { get; set; }
    }

    public class Order
    {
        public string id { get; set; }
        public string createTime { get; set; }
        public string type { get; set; }
        public string tradeID { get; set; }
        public string price { get; set; }
        public string timeInForce { get; set; }
        public string triggerCondition { get; set; }
        public string triggerMode { get; set; }
        public string state { get; set; }
    }

    public class Position
    {
        public string instrument { get; set; }
        public Long _long { get; set; }
        public Short _short { get; set; }
        public string pl { get; set; }
        public string resettablePL { get; set; }
        public string financing { get; set; }
        public string commission { get; set; }
        public string dividendAdjustment { get; set; }
        public string guaranteedExecutionFees { get; set; }
        public string unrealizedPL { get; set; }
    }

    public class Trade
    {
        public string id { get; set; }
        public string instrument { get; set; }
        public string price { get; set; }
        public string openTime { get; set; }
        public string initialUnits { get; set; }
        public string initialMarginRequired { get; set; }
        public string state { get; set; }
        public string currentUnits { get; set; }
        public string realizedPL { get; set; }
        public string financing { get; set; }
        public string dividendAdjustment { get; set; }
        public string unrealizedPL { get; set; }
        public string marginUsed { get; set; }
    }

    public class Long
    {
        public string units { get; set; }
        public string pl { get; set; }
        public string resettablePL { get; set; }
        public string financing { get; set; }
        public string dividendAdjustment { get; set; }
        public string guaranteedExecutionFees { get; set; }
        public string unrealizedPL { get; set; }
    }

    public class Short
    {
        public string units { get; set; }
        public string pl { get; set; }
        public string resettablePL { get; set; }
        public string financing { get; set; }
        public string dividendAdjustment { get; set; }
        public string guaranteedExecutionFees { get; set; }
        public string unrealizedPL { get; set; }
    }
}
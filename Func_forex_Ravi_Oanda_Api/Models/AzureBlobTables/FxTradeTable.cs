using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables
{
    public class FxTradeTable : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Id { get; set; }
        public string Instrument { get; set; }
        public string Price { get; set; }
        public string openTimeUTC { get; set; }
        public string openTimeEST { get; set; }
        public string initialUnits { get; set; }
        public string initialMarginRequired { get; set; }
        public string state { get; set; }
        public string currentUnits { get; set; }
        public string stopLossOrderID { get; set; }
        public string trailingStopLossOrderID { get; set; }
        public string unrealizedPL { get; set; }
        public string marginUsed { get; set; }
    }   
}

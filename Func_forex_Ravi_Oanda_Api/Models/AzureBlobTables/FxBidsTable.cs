using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables
{
    public class FxBidsTable : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }        
        public string Instrument { get; set; }                
        public DateTimeOffset? CurrencyDateTimeUTC { get; set; }
        public DateTimeOffset? CurrencyDateTimeEST { get; set; }
        public string EMA_5_Crossed_EMA_20_From_Below_Session_Id { get; set; }
    }   
}

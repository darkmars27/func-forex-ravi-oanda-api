using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables
{
    public class FxCurrencyTable : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }        
        public string Instrument { get; set; }
        public string Granularity { get; set; }
        public DateTimeOffset? CurrencyDateTimeUTC { get; set; }
        public DateTimeOffset? CurrencyDateTimeEST { get; set; }
        public string Cash { get; set; }
        public string Margin { get; set; }
        public int? Leverage { get; set; }
        public decimal Bid_Open { get; set; }
        public decimal Bid_Close { get; set; }
        public decimal Price_Open { get; set; }
        public decimal Price_Close { get; set; }
        public decimal Price_High { get; set; }
        public decimal Price_Low { get; set; }       
        public decimal Ask_Open { get; set; }
        public decimal Ask_Close { get; set; }
        public decimal Spread { get; set; }
        public decimal SMA_5 { get; set; }
        public decimal SMA_10 { get; set; }
        public decimal SMA_20 { get; set; }
        public decimal SMA_50 { get; set; }
        public decimal EMA_5 { get; set; }
        public decimal EMA_10 { get; set; }
        public decimal EMA_20 { get; set; }
        public decimal EMA_50 { get; set; }    
        public bool? EMA_5_Crossed_EMA_20_From_Below { get; set; }
        public bool? EMA_5_Crossed_EMA_20_From_Above { get; set; }
        public DateTimeOffset? EMA_5_Crossed_EMA_20_From_Below_Dt { get; set; }
        public bool? EMA_20_Crossed_EMA_50_From_Below { get; set; }
        public bool? EMA_20_Crossed_EMA_50_From_Above { get; set; }
        public DateTimeOffset? EMA_20_Crossed_EMA_50_From_Below_Dt { get; set; }
        public decimal EMA_Diff_5_20_pips { get; set; }
        public decimal EMA_Diff_20_50_pips { get; set; }
        public decimal EMA_Diff_5_50_pips { get; set; }
        public decimal RSI_14 { get; set; }
        public decimal Avg_Current_Gain_14 { get; set; }
        public decimal Avg_Current_Loss_14 { get; set; }
        public DateTimeOffset? lastBuyTime { get; set; }
    }   
}

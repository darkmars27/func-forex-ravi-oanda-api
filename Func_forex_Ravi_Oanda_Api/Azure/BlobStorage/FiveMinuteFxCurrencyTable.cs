//using Azure.Data.Tables;
//using Func_forex_Ravi_Oanda_Api.Helpers;
//using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Func_forex_Ravi_Oanda_Api.Azure.BlobStorage
//{
//    public class FiveMinuteFxCurrencyTable
//    {
//        private TableClient tableClient { get; set; }
//        public FiveMinuteFxCurrencyTable(string tableName)
//        {
//            //var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
//            tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), tableName);
//            tableClient.CreateIfNotExists();
//        }
       
//        public FxCurrencyTable GetEntity(string partitionKey, string rowKey)
//        {
//            var record = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}' and RowKey eq '{rowKey}'").FirstOrDefault();
//            return ConvertToFxCurrencyTable(record);
//        }

//        public List<FxCurrencyTable> GetPreviousEntities(string partitionKey, int period)
//        {
//            var records = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").Take(period).ToList();
//            return records.Select(o => ConvertToFxCurrencyTable(o)).ToList();
//        }

//        public async Task InsertEntityAsync(List<FxCurrencyTable> data)
//        {
//            foreach (var row in data) 
//            {
//                var dict = TransformToDIct(row);
//                await tableClient.AddEntityAsync(new TableEntity(dict));                
//            }
//        }

//        //public async Task UpdateEntityAsync(List<FxCurrencyTable> data)
//        //{
//        //    foreach (var row in data)
//        //    {
//        //        var dict = TransformToDIct(row);
//        //        await tableClient.UpdateEntityAsync(new TableEntity(dict));
//        //    }
//        //}

//        public async Task UpsertEntityAsync(List<FxCurrencyTable> data)
//        {
//            foreach (var row in data)
//            {
//                var dict = TransformToDIct(row);
//                await tableClient.UpsertEntityAsync(new TableEntity(dict));
//            }
//        }

//        public Dictionary<string, object> TransformToDIct(FxCurrencyTable row)
//        {
//            var minDateTime = new DateTime(1999, 01, 01, 0, 0, 0);
//            minDateTime = DateTime.SpecifyKind(minDateTime, DateTimeKind.Utc);
//            DateTimeOffset minDtOffset = minDateTime;
//            var rowDict = new Dictionary<string, object>
//                {
//                    {"PartitionKey", row.PartitionKey },
//                    {"RowKey", row. RowKey },
//                    {"Instrument", row.Instrument },
//                    {"Granularity", row.Granularity },
//                    {"CurrencyDateTimeUTC", row.CurrencyDateTimeUTC },
//                    {"CurrencyDateTimeEST", row.CurrencyDateTimeEST.Value.ToString("yyyy-MM-ddTHH:mm:ss") },
//                    {"Cash", row.Cash },
//                    {"Margin", row.Margin },
//                    {"Leverage", Convert.ToInt32(row.Leverage)},
//                    {"Bid_Open", row.Bid_Open.ToString()},
//                    {"Bid_Close", row.Bid_Close.ToString()},
//                    {"Price_Open", row.Price_Open.ToString()},
//                    {"Price_Close", row.Price_Close.ToString()},
//                    {"Price_High", row.Price_High.ToString()},
//                    {"Price_Low", row.Price_Low.ToString()},
//                    {"Ask_Open", row.Ask_Open.ToString()},
//                    {"Ask_Close", row.Ask_Close.ToString()},
//                    {"Spread", row.Spread.ToString("0.00")},
//                    {"SMA_5", row.SMA_5.ToString()},
//                    {"SMA_10", row.SMA_10.ToString()},
//                    {"SMA_20", row.SMA_20.ToString()},
//                    {"SMA_50", row.SMA_50.ToString()},
//                    {"EMA_5", row.EMA_5.ToString()},
//                    {"EMA_10", row.EMA_10.ToString()},
//                    {"EMA_20", row.EMA_20.ToString()},
//                    {"EMA_50", row.EMA_50.ToString()},
//                    {"EMA_Diff_5_20_pips", row.EMA_Diff_5_20_pips.ToString("0.00")},
//                    {"EMA_Diff_20_50_pips", row.EMA_Diff_20_50_pips.ToString("0.00")},
//                    {"EMA_Diff_5_50_pips", row.EMA_Diff_5_50_pips.ToString("0.00")},
//                    {"RSI_14", row.RSI_14.ToString()},
//                    {"Avg_Current_Gain_14", row.Avg_Current_Gain_14.ToString()},
//                    {"Avg_Current_Loss_14", row.Avg_Current_Loss_14.ToString()},
//                    {"EMA_5_Crossed_EMA_20_From_Below", row.EMA_5_Crossed_EMA_20_From_Below.HasValue ? row.EMA_5_Crossed_EMA_20_From_Below.Value : false},
//                    {"EMA_5_Crossed_EMA_20_From_Above", row.EMA_5_Crossed_EMA_20_From_Above.HasValue ? row.EMA_5_Crossed_EMA_20_From_Above.Value : false},
//                    {"EMA_5_Crossed_EMA_20_From_Below_Dt", row.EMA_5_Crossed_EMA_20_From_Below_Dt.HasValue ? row.EMA_5_Crossed_EMA_20_From_Below_Dt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : minDtOffset.ToUniversalTime()},
//                    {"EMA_20_Crossed_EMA_50_From_Below", row.EMA_20_Crossed_EMA_50_From_Below.HasValue ? row.EMA_20_Crossed_EMA_50_From_Below.Value : false},
//                    {"EMA_20_Crossed_EMA_50_From_Above", row.EMA_20_Crossed_EMA_50_From_Above.HasValue ? row.EMA_20_Crossed_EMA_50_From_Above.Value : false},
//                    {"EMA_20_Crossed_EMA_50_From_Below_Dt", row.EMA_20_Crossed_EMA_50_From_Below_Dt.HasValue ? row.EMA_20_Crossed_EMA_50_From_Below_Dt.Value.ToString("yyyy-MM-ddTHH:mm:ss") : minDtOffset.ToUniversalTime()},
//                    {"lastBuyTime", row.lastBuyTime.HasValue ? row.lastBuyTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null},
//                };
//            return rowDict;
//        }

//        public FxCurrencyTable ConvertToFxCurrencyTable(TableEntity record)
//        {
//            FxCurrencyTable table = new FxCurrencyTable
//            {
//                PartitionKey = record.PartitionKey,
//                RowKey = record.RowKey,
//                Timestamp = record.Timestamp,
//                ETag = record.ETag,
//                Instrument = record.GetString("Instrument"),
//                Granularity = record.GetString("Granularity"),
//                CurrencyDateTimeUTC = record.GetDateTimeOffset("CurrencyDateTimeUTC"),
//                CurrencyDateTimeEST = DateTimeOffset.Parse(record.GetString("CurrencyDateTimeEST")),
//                Cash = record.GetString("Cash"),
//                Margin = record.GetString("Margin"),
//                Leverage = record.GetInt32("Leverage"),
//                Bid_Open = decimal.Parse(record.GetString("Bid_Open")),
//                Bid_Close = decimal.Parse(record.GetString("Bid_Close")),
//                Price_Open = decimal.Parse(record.GetString("Price_Open")),
//                Price_Close = decimal.Parse(record.GetString("Price_Close")),
//                Price_High = decimal.Parse(record.GetString("Price_High")),
//                Price_Low = decimal.Parse(record.GetString("Price_Low")),
//                Ask_Open = decimal.Parse(record.GetString("Ask_Open")),
//                Ask_Close = decimal.Parse(record.GetString("Ask_Close")),
//                Spread = decimal.Parse(record.GetString("Spread")),
//                SMA_5 = decimal.Parse(record.GetString("SMA_5")),
//                SMA_10 = decimal.Parse(record.GetString("SMA_10")),
//                SMA_20 = decimal.Parse(record.GetString("SMA_20")),
//                SMA_50 = decimal.Parse(record.GetString("SMA_50")),
//                EMA_5 = decimal.Parse(record.GetString("EMA_5")),
//                EMA_10 = decimal.Parse(record.GetString("EMA_10")),
//                EMA_20 = decimal.Parse(record.GetString("EMA_20")),
//                EMA_50 = decimal.Parse(record.GetString("EMA_50")),
//                EMA_Diff_5_20_pips = decimal.Parse(record.GetString("EMA_Diff_5_20_pips")),
//                EMA_Diff_20_50_pips = decimal.Parse(record.GetString("EMA_Diff_20_50_pips")),
//                EMA_Diff_5_50_pips = decimal.Parse(record.GetString("EMA_Diff_5_50_pips")),
//                RSI_14 = decimal.Parse(record.GetString("RSI_14")),
//                Avg_Current_Gain_14 = decimal.Parse(record.GetString("Avg_Current_Gain_14")),
//                Avg_Current_Loss_14 = decimal.Parse(record.GetString("Avg_Current_Loss_14")),
//                EMA_5_Crossed_EMA_20_From_Above = record.GetBoolean("EMA_5_Crossed_EMA_20_From_Above"),
//                EMA_5_Crossed_EMA_20_From_Below = record.GetBoolean("EMA_5_Crossed_EMA_20_From_Below"),
//                EMA_5_Crossed_EMA_20_From_Below_Dt = record.GetString("EMA_5_Crossed_EMA_20_From_Below_Dt").ConvertToDateTimeOffset(),
//                EMA_20_Crossed_EMA_50_From_Below = record.GetBoolean("EMA_20_Crossed_EMA_50_From_Below"),
//                EMA_20_Crossed_EMA_50_From_Above = record.GetBoolean("EMA_20_Crossed_EMA_50_From_Above"),
//                EMA_20_Crossed_EMA_50_From_Below_Dt = record.GetString("EMA_20_Crossed_EMA_50_From_Below_Dt").ConvertToDateTimeOffset(),
//                lastBuyTime = record.GetString("lastBuyTime").ConvertToDateTimeOffset()
//            };
//            return table;
//        }
//    }
//}

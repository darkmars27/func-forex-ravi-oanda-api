using Azure.Data.Tables;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Azure.BlobStorage
{
    public class FxCurrencyTableHelpers
    {
        private TableClient tableClient { get; set; }
        //private readonly string tableName = "oandaforexdatademo";
        public FxCurrencyTableHelpers(string tableName)
        {
            //var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), tableName);
            tableClient.CreateIfNotExists();
        }
       
        public FxCurrencyTable GetEntity(string partitionKey, string rowKey)
        {
            var record = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}' and RowKey eq '{rowKey}'").FirstOrDefault();
            return ConvertToFxCurrencyTable(record);
        }

        public List<FxCurrencyTable> GetPreviousEntities(string partitionKey, int period)
        {
            var records = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").Take(period).ToList();
            return records.Select(o => ConvertToFxCurrencyTable(o)).ToList();
        }       

        public async Task InsertEntityAsync(List<FxCurrencyTable> data)
        {
            foreach (var row in data) 
            {
                var dict = TransformToDIct(row);
                await tableClient.AddEntityAsync(new TableEntity(dict));                
            }
        }

        //public async Task UpdateEntityAsync(List<FxCurrencyTable> data)
        //{
        //    foreach (var row in data)
        //    {
        //        var dict = TransformToDIct(row);
        //        await tableClient.UpdateEntityAsync(new TableEntity(dict));
        //    }
        //}

        public async Task UpsertEntityAsync(List<FxCurrencyTable> data)
        {
            foreach (var row in data)
            {
                var dict = TransformToDIct(row);
                await tableClient.UpsertEntityAsync(new TableEntity(dict));
            }
        }

        public Dictionary<string, object> TransformToDIct(FxCurrencyTable row)
        {
            
            var rowDict = new Dictionary<string, object>
                {
                    {"PartitionKey", row.PartitionKey },
                    {"RowKey", row.RowKey },
                    {"Instrument", row.Instrument },
                    {"Granularity", row.Granularity },
                    {"CurrencyDateTimeUTC", row.CurrencyDateTimeUTC },
                    {"CurrencyDateTimeEST", row.CurrencyDateTimeEST.Value.ToString("yyyy-MM-ddTHH:mm:ss") },
                    {"Margin", row.Margin },
                    {"Leverage", Convert.ToInt32(row.Leverage)},
                    {"Bid_Open", row.Bid_Open.ToString()},
                    {"Bid_Close", row.Bid_Close.ToString()},
                    {"Price_Open", row.Price_Open.ToString()},
                    {"Price_Close", row.Price_Close.ToString()},
                    {"Price_High", row.Price_High.ToString()},
                    {"Price_Low", row.Price_Low.ToString()},
                    {"Ask_Open", row.Ask_Open.ToString()},
                    {"Ask_Close", row.Ask_Close.ToString()},
                    {"Spread", row.Spread.ToString("0.00")},
                    {"SMA_5", row.SMA_5.ToString()},
                    {"SMA_10", row.SMA_10.ToString()},
                    {"SMA_50", row.SMA_50.ToString()},
                    {"EMA_5", row.EMA_5.ToString()},
                    {"EMA_10", row.EMA_10.ToString()},
                    {"EMA_50", row.EMA_50.ToString()},
                    {"EMA_5_10_Below", row.EMA_5_Crossed_EMA_10_From_Below.GetValueOrDefault()},
                    {"EMA_5_10_Above", row.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault()},
                    {"EMA_10_50_Below", row.EMA_10_Crossed_EMA_50_From_Below.GetValueOrDefault()},
                    {"EMA_10_50_Above", row.EMA_10_Crossed_EMA_50_From_Above.GetValueOrDefault()},
                    {"EMA_5_50_Below", row.EMA_5_Crossed_EMA_50_From_Below.GetValueOrDefault()},
                    {"EMA_5_50_Above", row.EMA_5_Crossed_EMA_50_From_Above.GetValueOrDefault()},
                    {"EMA_5_10_Below_Dt", row.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueString()},
                    {"EMA_5_10_Above_Dt", row.EMA_5_Crossed_EMA_10_From_Above_Dt.GetValueString()},
                    {"EMA_10_50_Below_Dt", row.EMA_10_Crossed_EMA_50_From_Below_Dt.GetValueString()},
                    {"EMA_10_50_Above_Dt", row.EMA_10_Crossed_EMA_50_From_Above_Dt.GetValueString()},
                    {"EMA_5_50_Below_Dt", row.EMA_5_Crossed_EMA_50_From_Below_Dt.GetValueString()},
                    {"EMA_5_50_Above_Dt", row.EMA_5_Crossed_EMA_50_From_Above_Dt.GetValueString()},
                    {"EMA_5_10_Spread", (row.EMA_5 - row.EMA_10).ToString()},
                    {"EMA_10_50_Spread", (row.EMA_10 - row.EMA_50).ToString()},
                    {"EMA_5_50_Spread", (row.EMA_5 - row.EMA_50).ToString()},
                    {"RSI_14", row.RSI_14.ToString()},
                    {"Avg_Current_Gain_14", row.Avg_Current_Gain_14.ToString()},
                    {"Avg_Current_Loss_14", row.Avg_Current_Loss_14.ToString()},
                };
            return rowDict;
        }

        public FxCurrencyTable ConvertToFxCurrencyTable(TableEntity record)
        {
            FxCurrencyTable table = new FxCurrencyTable
            {
                PartitionKey = record.PartitionKey,
                RowKey = record.RowKey,
                Timestamp = record.Timestamp,
                ETag = record.ETag,
                Instrument = record.GetString("Instrument"),
                Granularity = record.GetString("Granularity"),
                CurrencyDateTimeUTC = record.GetDateTimeOffset("CurrencyDateTimeUTC"),
                CurrencyDateTimeEST = DateTimeOffset.Parse(record.GetString("CurrencyDateTimeEST")),
                Margin = record.GetString("Margin"),
                Leverage = record.GetInt32("Leverage"),
                Bid_Open = decimal.Parse(record.GetString("Bid_Open")),
                Bid_Close = decimal.Parse(record.GetString("Bid_Close")),
                Price_Open = decimal.Parse(record.GetString("Price_Open")),
                Price_Close = decimal.Parse(record.GetString("Price_Close")),
                Price_High = decimal.Parse(record.GetString("Price_High")),
                Price_Low = decimal.Parse(record.GetString("Price_Low")),
                Ask_Open = decimal.Parse(record.GetString("Ask_Open")),
                Ask_Close = decimal.Parse(record.GetString("Ask_Close")),
                Spread = decimal.Parse(record.GetString("Spread")),
                SMA_5 = decimal.Parse(record.GetString("SMA_5")),
                SMA_10 = decimal.Parse(record.GetString("SMA_10")),
                SMA_50 = decimal.Parse(record.GetString("SMA_50")),
                EMA_5 = decimal.Parse(record.GetString("EMA_5")),
                EMA_10 = decimal.Parse(record.GetString("EMA_10")),
                EMA_50 = decimal.Parse(record.GetString("EMA_50")),
                EMA_5_Crossed_EMA_10_From_Below = record.GetBoolean("EMA_5_10_Below"),
                EMA_5_Crossed_EMA_10_From_Above = record.GetBoolean("EMA_5_10_Above"),
                EMA_10_Crossed_EMA_50_From_Below = record.GetBoolean("EMA_10_50_Below"),
                EMA_10_Crossed_EMA_50_From_Above = record.GetBoolean("EMA_10_50_Above"),
                EMA_5_Crossed_EMA_50_From_Below = record.GetBoolean("EMA_5_50_Below"),
                EMA_5_Crossed_EMA_50_From_Above = record.GetBoolean("EMA_5_50_Above"),
                EMA_5_Crossed_EMA_10_From_Below_Dt = record.GetString("EMA_5_10_Below_Dt").ConvertToDateTimeOffset(),
                EMA_5_Crossed_EMA_10_From_Above_Dt = record.GetString("EMA_5_10_Above_Dt").ConvertToDateTimeOffset(),
                EMA_10_Crossed_EMA_50_From_Below_Dt = record.GetString("EMA_10_50_Below_Dt").ConvertToDateTimeOffset(),
                EMA_10_Crossed_EMA_50_From_Above_Dt = record.GetString("EMA_10_50_Above_Dt").ConvertToDateTimeOffset(),
                EMA_5_Crossed_EMA_50_From_Below_Dt = record.GetString("EMA_5_50_Below_Dt").ConvertToDateTimeOffset(),
                EMA_5_Crossed_EMA_50_From_Above_Dt = record.GetString("EMA_5_50_Above_Dt").ConvertToDateTimeOffset(),
                EMA_5_10_Spread = decimal.Parse(record.GetString("EMA_5_10_Spread")),
                EMA_10_50_Spread = decimal.Parse(record.GetString("EMA_10_50_Spread")),
                EMA_5_50_Spread = decimal.Parse(record.GetString("EMA_5_50_Spread")),
                RSI_14 = decimal.Parse(record.GetString("RSI_14")),
                Avg_Current_Gain_14 = decimal.Parse(record.GetString("Avg_Current_Gain_14")),
                Avg_Current_Loss_14 = decimal.Parse(record.GetString("Avg_Current_Loss_14")),
            };
            return table;
        }
    }
}

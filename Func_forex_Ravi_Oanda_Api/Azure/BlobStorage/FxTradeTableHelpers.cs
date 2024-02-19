using Azure.Data.Tables;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Azure.BlobStorage
{
    public class FxTradeTableHelpers
    {
        private TableClient tableClient { get; set; }
        private readonly string tableName = "fxtradedemo";
        public FxTradeTableHelpers()
        {
            //var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), tableName);
            tableClient.CreateIfNotExists();
        }
              
        public List<FxTradeTable> GetPreviousEntities(string partitionKey, int period)
        {
            var records = tableClient.Query<FxTradeTable>(filter: $"PartitionKey eq '{partitionKey}'").Take(period).ToList();
            return records;
        }

        public async Task UpsertEntityAsync(FxTradeTable data)
        {
            await tableClient.UpsertEntityAsync(data);
        }

        public FxTradeTable TransformToDIct(FxTradeTable row)
        {
            var utcTime = DateTime.Parse(row.openTimeUTC);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            row.PartitionKey = row.Instrument;
            row.RowKey = (DateTime.MaxValue.Ticks - utcTime.Ticks).ToString();
            row.openTimeEST = TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternZone).ToString();
            return row;
        }       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Func_forex_Ravi_Oanda_Api.Azure.BlobStorage
{
    public class BlobStorageTableHelpers
    {
        private TableClient tableClient { get; set; }
        private readonly string tableName = "oandaforexdata";
        public BlobStorageTableHelpers()
        {
            //var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), tableName);
            tableClient.CreateIfNotExists();
        }
       
        public FxCurrencyTable GetEntity(string partitionKey, string rowKey)
        {
            var record = tableClient.Query<FxCurrencyTable>(filter: $"PartitionKey eq '{partitionKey}' and RowKey eq '{rowKey}'").FirstOrDefault();
            return record;
        }

        public List<FxCurrencyTable> GetPreviousEntity(string partitionKey, DateTimeOffset datetime, int limit)
        {
            var records = tableClient.Query<FxCurrencyTable>(filter: $"PartitionKey eq '{partitionKey}' and CurrencyDateTimeUTC lt datetime'{datetime.ToString("yyyy-MM-ddTHH:mm:ssZ")}'").OrderBy(o => o.CurrencyDateTimeUTC).TakeLast(limit).ToList();
            return records;
        }

        public async Task InsertEntityAsync(List<FxCurrencyTable> data)
        {
            foreach (var row in data) 
            {
                var jsonString = JsonConvert.SerializeObject(row);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                dict["CurrencyDateTimeUTC"] = row.CurrencyDateTimeUTC;
                dict.Remove("Timestamp");
                dict.Remove("ETag");
                var tableEntity = new TableEntity(dict);
                tableEntity.RowKey = row.RowKey;
                tableEntity.PartitionKey = row.PartitionKey;
                await tableClient.AddEntityAsync(tableEntity);                
            }
        }

        public async Task UpsertEntityAsync(string partitionKey, string rowKey, List<FxCurrencyTable> data)
        {
            foreach (var row in data)
            {
                var jsonString = JsonConvert.SerializeObject(row);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                dict["CurrencyDateTimeUTC"] = row.CurrencyDateTimeUTC;
                dict.Remove("Timestamp");
                dict.Remove("ETag");
                var tableEntity = new TableEntity(dict);
                tableEntity.RowKey = row.RowKey;
                tableEntity.PartitionKey = partitionKey;
                await tableClient.UpsertEntityAsync(tableEntity);
            }
        }
    }
}

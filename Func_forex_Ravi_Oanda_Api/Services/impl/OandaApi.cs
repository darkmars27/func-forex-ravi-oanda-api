using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using Func_forex_Ravi_Oanda_Api.Models;
using System.Linq;
namespace Func_forex_Ravi_Oanda_Api.Services.Impl
{
    
    public class OandaApi : IOandaApi
    {
        string accountId = "101-001-28144762-001";
        string granularity5Min = "M5";
        string granularity15Min = "M15";
        string price = "AMB";
        bool smooth = false;
        private readonly HttpClient httpClient;

        public OandaApi(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("OandaApiHttpClient");
        }

        public async Task<PricingModel> GetPriceHistory5Min(string instrument_name)
        {
            string url = $"v3/accounts/{accountId}/instruments/{instrument_name}/candles?granularity={granularity5Min}&price={price}&smooth={smooth}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var pricingHistory = JsonConvert.DeserializeObject<PricingModel>(jsonResponse);
            return pricingHistory;
        }

        public async Task<PricingModel> GetPriceHistory15Min(string instrument_name)
        {
            string url = $"v3/accounts/{accountId}/instruments/{instrument_name}/candles?granularity={granularity15Min}&price={price}&smooth={smooth}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var pricingHistory = JsonConvert.DeserializeObject<PricingModel>(jsonResponse);
            return pricingHistory;
        }

        public async Task<PricingLatestModel> GetLatestPrice5Min(params string[] instrument_names)
        {
            instrument_names = instrument_names.Select(o => $"{o}:{granularity5Min}:{price}").ToArray();
            string candleSpecifications = string.Join(",", instrument_names);
            string url = $"/v3/accounts/{accountId}/candles/latest?candleSpecifications={candleSpecifications}&smooth={smooth}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var latestPricing = JsonConvert.DeserializeObject<PricingLatestModel>(jsonResponse);
            return latestPricing;
        }

        public async Task<PricingLatestModel> GetLatestPrice15Min(params string[] instrument_names)
        {
            instrument_names = instrument_names.Select(o => $"{o}:{granularity15Min}:{price}").ToArray();
            string candleSpecifications = string.Join(",", instrument_names);
            string url = $"/v3/accounts/{accountId}/candles/latest?candleSpecifications={candleSpecifications}&smooth={smooth}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var latestPricing = JsonConvert.DeserializeObject<PricingLatestModel>(jsonResponse);
            return latestPricing;
        }

        public async Task<AccountModel> GetAccount()
        {
            string url = $"/v3/accounts/{accountId}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var account = JsonConvert.DeserializeObject<AccountModel>(jsonResponse);
            return account;
        }

        public async Task<MarketOrderResponse> PostMarketOrderRequest(MarketOrderRequest orderRequest)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(orderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders";
            using HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var mrktOrderResponse = JsonConvert.DeserializeObject<MarketOrderResponse>(responseString);
                return mrktOrderResponse;
            }
            else
                return null;
        }

        public async Task<bool> PostTrailingStopLossRequest(TrailingStopLossRequest orderRequest)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(orderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders";
            using HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        public async Task<bool> PutCloseTradeRequest(TradeCloseRequest tradeCloseRequest, string tradeId)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(tradeCloseRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/trades/{tradeId}/close";
            using HttpResponseMessage response = await httpClient.PutAsync(url, stringContent);
            if(response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        public async Task<MarketOrders> GetFilledMarketOrders(string instrument)
        {
            string url = $"/v3/accounts/{accountId}/orders?state=ALL&instrument={instrument}&count=10";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var marketOrders = JsonConvert.DeserializeObject<MarketOrders>(jsonResponse);
            if(marketOrders != null && marketOrders.orders != null && marketOrders.orders.Any())
                marketOrders.orders = marketOrders.orders.Where(o => o.type == "MARKET" && o.state == "FILLED").ToList();

            return marketOrders;
        }
    }
}
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System;
namespace Ravi.Oanda.Automation
{
    
    public class OandaApi
    {
        
        string accountId = "101-001-28144762-001";
        string granularity = "M15";
        string price = "AMB";
        bool smooth = false;
        private HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api-fxpractice.oanda.com")
        };

        public OandaApi()
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer a52d404534b4a81089f19de177b95086-20cb29411271c72665147a8863dcc742");
        }
        public async Task<PricingModel> GetPriceHistory(string instrument_name)
        {
            string url = $"v3/accounts/{accountId}/instruments/{instrument_name}/candles?granularity={granularity}&price={price}&smooth={smooth}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var pricingHistory = JsonConvert.DeserializeObject<PricingModel>(jsonResponse);
            return pricingHistory;
        }

        public async Task<PricingLatestModel> GetLatestPrice(string instrument_name)
        {
            string url = $"/v3/accounts/{accountId}/candles/latest?candleSpecifications={instrument_name}:{granularity}:{price}&smooth={smooth}";
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

        public async Task<bool> PostOrderRequest(OrderRequest orderRequest)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(orderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders";
            using HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if(response.IsSuccessStatusCode)
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
    }
}
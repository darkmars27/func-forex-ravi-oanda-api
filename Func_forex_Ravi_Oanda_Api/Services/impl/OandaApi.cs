using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using Func_forex_Ravi_Oanda_Api.Models;
using System.Linq;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using System.Security.Principal;
namespace Func_forex_Ravi_Oanda_Api.Services.Impl
{
    
    public class OandaApi : IOandaApi
    {
        string accountId = "101-001-28144762-001";
        string granularity5Min = "M5";
        string granularity15Min = "M15";
        string price = "AMB";
        bool smooth = false;
        decimal pip = 0.0001m;

        private readonly HttpClient httpClient;
        private readonly ILogger<OandaApi> log;

        public OandaApi(IHttpClientFactory httpClientFactory, ILogger<OandaApi> log)
        {
            httpClient = httpClientFactory.CreateClient("OandaApiHttpClient");
            this.log = log;
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
            string url = $"v3/instruments/{instrument_name}/candles?granularity={granularity15Min}&price={price}&smooth={smooth}&count=5000";
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

        public async Task<AccountInstrumentDetail> GetAccountInstrumentDetails(params string[] instruments)
        {
            var instrumentscsv = string.Join(",", instruments);
            string url = $"/v3/accounts/{accountId}/instruments?instruments={instrumentscsv}";
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            var res = JsonConvert.DeserializeObject<AccountInstrumentDetail>(jsonResponse);
            return res;
        }

        public async Task<string> PostMarketOrderRequest(string instrument_name, decimal cash, int leverage, decimal ask_close, decimal close_price, decimal stop_loss_price)
        {
            var orderRequest = new MarketOrderRequest
            {
                order = new MarketOrderRequest.MarketOrder
                {
                    type = "MARKET",
                    instrument = instrument_name,
                    units = Convert.ToInt32(cash * leverage / ask_close),
                    stopLossOnFill = new MarketOrderRequest.StopLossOnFill
                    {
                        price = stop_loss_price.ToString() //(close_price - pip * 15).ToString()
                    }
                }
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(orderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders";
            using HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var mrktOrderResponse = JsonConvert.DeserializeObject<MarketOrderResponse>(responseString);
                if (mrktOrderResponse != null || !string.IsNullOrEmpty(mrktOrderResponse.orderFillTransaction?.id))
                {
                    log.LogInformation($"{instrument_name} - {mrktOrderResponse.orderFillTransaction?.id} New Market Order Placed, Trade ID: {mrktOrderResponse.orderFillTransaction?.id}, Close/Ask Price: {close_price}/{ask_close}, Response : {responseString}");
                    return mrktOrderResponse.orderFillTransaction?.id;
                }
            }
            log.LogError($"{instrument_name} Failed To Put Market Order, Response: {await TryReadAsStringAsync(response)}");
            return null;
        }

        public async Task<bool> PostTrailingStopLossRequest(string instrument_name, string tradeId, decimal distance)
        {
            var trailingStopOrderRequest = new TrailingStopLossRequest
            {
                order = new TrailingStopLossRequestOrder
                {
                    type = "TRAILING_STOP_LOSS",
                    tradeID = tradeId,
                    distance = distance.ToString("0.00000"),
                    timeInForce = "GTC"
                }
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(trailingStopOrderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders";
            using HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"{instrument_name} - {tradeId} Trailing Stop Loss {trailingStopOrderRequest.order.distance} On TradeId {tradeId}, Response: {await TryReadAsStringAsync(response)}");
                return true;
            }
            else
            {
                log.LogError($"{instrument_name} - {tradeId} Trailing Stop Loss Failed {trailingStopOrderRequest.order.distance} On TradeId {tradeId}, Response: {await TryReadAsStringAsync(response)}");
                return false;
            }
        }

        public async Task<bool> PutTrailingStopLossRequest(string instrument_name, string tradeId, string orderId, decimal distance)
        {
            var trailingStopOrderRequest = new TrailingStopLossRequest
            {
                order = new TrailingStopLossRequestOrder
                {
                    type = "TRAILING_STOP_LOSS",
                    tradeID = tradeId,
                    distance = distance.ToString("0.00000"),
                    timeInForce = "GTC"
                }
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(trailingStopOrderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders/{orderId}";
            using HttpResponseMessage response = await httpClient.PutAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"{instrument_name} - {tradeId} Trailing Stop Loss Updated {trailingStopOrderRequest.order.distance} On TradeId {tradeId}, Trailing Stop Loss OrderId {orderId}, Response: {await TryReadAsStringAsync(response)}");
                return true;
            }
            else
            {
                log.LogError($"{instrument_name} - {tradeId} Trailing Stop Loss Update Failed {trailingStopOrderRequest.order.distance} On TradeId {tradeId}, Trailing Stop Loss OrderId {orderId}, Response: {await TryReadAsStringAsync(response)}");
                return false;
            }
        }


        public async Task<bool> PutStopLossRequest(string instrument_name, string tradeId, string orderId, decimal price)
        {
            var stopOrderRequest = new StopLossRequest
            {
                order = new StopLossRequestOrder
                {
                    type = "STOP_LOSS",
                    tradeID = tradeId,
                    price = price.ToString("0.00000"),
                    timeInForce = "GTC"
                }
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(stopOrderRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/orders/{orderId}";
            using HttpResponseMessage response = await httpClient.PutAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"{instrument_name} - {tradeId} Stop Loss Updated {stopOrderRequest.order.price} On TradeId {tradeId}, Stop Loss OrderId {orderId}, Response: {await TryReadAsStringAsync(response)}");
                return true;
            }
            else
            {
                log.LogError($"{instrument_name} - {tradeId} Stop Loss Update Failed {stopOrderRequest.order.price} On TradeId {tradeId}, Stop Loss OrderId {orderId}, Response: {await TryReadAsStringAsync(response)}");
                return false;
            }
        }

        public async Task<bool> PutCloseTradeRequest(string instrument_name, string tradeId, int units = 0)
        {
            var tradeCloseRequest = new TradeCloseRequest
            {
                units =  units == 0 ? "ALL" : units.ToString()
            };

            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(tradeCloseRequest), System.Text.Encoding.UTF8, "application/json");
            string url = $"/v3/accounts/{accountId}/trades/{tradeId}/close";
            using HttpResponseMessage response = await httpClient.PutAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"{instrument_name} - {tradeId} Closed, Response: {await TryReadAsStringAsync(response)}");
                return true;
            }
            else
            {
                log.LogError($"{instrument_name} - {tradeId} Close Failed, Error: {await TryReadAsStringAsync(response)}");
                return false;
            }
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

        private async Task<string> TryReadAsStringAsync(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
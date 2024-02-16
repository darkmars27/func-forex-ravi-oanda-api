using Func_forex_Ravi_Oanda_Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.Services
{
    public interface IOandaApi
    {
        Task<PricingModel> GetPriceHistory5Min(string instrument_name);
        Task<PricingModel> GetPriceHistory15Min(string instrument_name);
        Task<PricingLatestModel> GetLatestPrice5Min(params string[] instrument_names);
        Task<PricingLatestModel> GetLatestPrice15Min(params string[] instrument_names);
        Task<AccountModel> GetAccount();
        Task<AccountInstrumentDetail> GetAccountInstrumentDetails(params string[] instruments);
        Task<string> PostMarketOrderRequest(string instrument_name, decimal cash, int leverage, decimal ask_close, decimal close_price, decimal stop_loss_price);
        Task<bool> PostTrailingStopLossRequest(string instrument_name, string tradeId, decimal distance);
        Task<bool> PutTrailingStopLossRequest(string instrument_name, string tradeId, string orderId, decimal distance);
        Task<bool> PutStopLossRequest(string instrument_name, string tradeId, string orderId, decimal price);
        Task<bool> PutCloseTradeRequest(string instrument_name, string tradeId, int units = 0);
        Task<MarketOrders> GetFilledMarketOrders(string instrument);
    }
}

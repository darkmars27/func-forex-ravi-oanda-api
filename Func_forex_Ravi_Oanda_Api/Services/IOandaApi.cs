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
        Task<PricingModel> GetPriceHistory(string instrument_name);
        Task<PricingLatestModel> GetLatestPrice(params string[] instrument_names);
        Task<AccountModel> GetAccount();
        Task<bool> PostOrderRequest(OrderRequest orderRequest);
        Task<bool> PutCloseTradeRequest(TradeCloseRequest tradeCloseRequest, string tradeId);
    }
}

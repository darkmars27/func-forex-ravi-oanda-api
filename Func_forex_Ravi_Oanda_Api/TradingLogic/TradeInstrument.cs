using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic
{
    public class TradeInstrument
    {
        private readonly ILogger<TradeInstrument> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        public TradeInstrument(ILogger<TradeInstrument> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
        }
        public async Task Run(string instrument_name)
        {
            try
            {
                Account account = null;
                string marginRate = null;
                var accountDetail = await oandaApi.GetAccount();
                if (accountDetail != null)
                    account = accountDetail.account;

                var accountInstrumentDetail = await oandaApi.GetAccountInstrumentDetails(instrument_name);
                if (accountInstrumentDetail != null && accountInstrumentDetail.instruments.Count > 0)
                    marginRate = accountInstrumentDetail.instruments[0].marginRate;

                if (account == null || string.IsNullOrEmpty(marginRate))
                {
                    log.LogError($"{instrument_name} Skipping Trading As Failed To Read Account & Instrument Margin Rate");
                    return;
                }

                var fxdata = tableHelpers.GetPreviousEntities(instrument_name, 2);
                fxdata = fxdata.OrderByDescending(o => o.RowKey).ToList();
                var previous = fxdata[0];
                var current = fxdata[1];
                // Checking If ClosePrice, EMA 5, EMA_10 Cross EMA_50
                current = current.DoEMATrendAnalysis(previous);
                if (account.openTradeCount > 0)
                {
                    if (account.trades[0].instrument == instrument_name)
                    {
                        if (current.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault() ||
                            current.EMA_10_Crossed_EMA_50_From_Above.GetValueOrDefault() ||
                            current.EMA_5_Crossed_EMA_50_From_Above.GetValueOrDefault() ||
                            Converters.GetCurrentEasternTime().Hour == 7)
                        {
                            await oandaApi.PutCloseTradeRequest(instrument_name, account.trades[0].id);
                        }
                    }
                }
                else
                {
                    if (current.Price_Close > current.EMA_50 &&
                        current.EMA_5_Crossed_EMA_10_From_Below.GetValueOrDefault() &&
                        current.EMA_10_Crossed_EMA_50_From_Below.GetValueOrDefault() &&
                        current.EMA_5_Crossed_EMA_50_From_Below.GetValueOrDefault())
                    {
                        bool skipMarketOrder = false;
                        var filledMarketOrders = await oandaApi.GetFilledMarketOrders(instrument_name);
                        if (filledMarketOrders != null && filledMarketOrders.orders != null && filledMarketOrders.orders.Any())
                        {
                            if (filledMarketOrders.orders.Any(o => o.createTime > current.EMA_5_Crossed_EMA_10_From_Below_Dt.GetValueOrDefault()))
                            {
                                skipMarketOrder = true;
                                log.LogInformation($"{instrument_name} Skipping Market Order (Second Market Order In Same EMA Cross)");
                            }
                        }

                        if (!skipMarketOrder)
                        {
                            var latestPrice = await oandaApi.GetLatestPrice15Min(instrument_name);
                            var current_ask_price = latestPrice.LatestCandles[0].Candles.OrderByDescending(o => o.Time).FirstOrDefault().Ask.C;
                            var tradeId = await oandaApi.PostMarketOrderRequest(instrument_name, account.balance, current.Leverage.Value, current_ask_price);
                            if(!string.IsNullOrEmpty(tradeId))
                                await oandaApi.PostTrailingStopLossRequest(instrument_name, tradeId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{instrument_name} Failed To Put Trade Order, Exception {ex.ToString()}");
            }
        }
    }
}

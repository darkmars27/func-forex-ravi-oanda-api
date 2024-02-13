using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Helpers;
using Func_forex_Ravi_Oanda_Api.Models;
using Func_forex_Ravi_Oanda_Api.Models.AzureBlobTables;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.TradingLogic.Indicators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Func_forex_Ravi_Oanda_Api.TradingLogic
{
    public class TradeInstrument
    {
        private readonly ILogger<TradeInstrument> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        decimal pip = 0.0001m;
        public TradeInstrument(ILogger<TradeInstrument> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
        }
        public async Task Run(string instrument_name, List<FxCurrencyTable> fxdata)
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
                                
                fxdata = fxdata.OrderByDescending(o => o.RowKey).ToList();
                fxdata = fxdata.TakeLast(2).ToList();
                var previous = fxdata[0];
                var current = fxdata[1];
                if (account.openTradeCount > 0)
                {
                    if (account.trades[0].instrument == instrument_name)
                    {
                        if (current.EMA_5_Crossed_EMA_10_From_Above.GetValueOrDefault())
                            await oandaApi.PutCloseTradeRequest(instrument_name, account.trades[0].id);
                        else if (current.RSI_14 >= 70)
                            await oandaApi.PutCloseTradeRequest(instrument_name, account.trades[0].id);
                        else if(Converters.GetCurrentEasternTime().Hour == 7 && Converters.GetCurrentEasternTime().Minute > 30)
                            await oandaApi.PutCloseTradeRequest(instrument_name, account.trades[0].id);
                    }
                }
                else
                {
                    if (current.EMA_5_Crossed_EMA_10_From_Below.GetValueOrDefault() && Converters.GetPips(current.EMA_5, current.EMA_10) >= 1 && current.RSI_14 < 65)
                    {
                        log.LogInformation($"EMA_5: {current.EMA_5}");
                        log.LogInformation($"EMA_10: {current.EMA_10}");
                        log.LogInformation($"Converters.GetPips(current.EMA_5, current.EMA_10): {Converters.GetPips(current.EMA_5, current.EMA_10)}");
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
                            var tradeId = await oandaApi.PostMarketOrderRequest(instrument_name, account.balance, current.Leverage.Value, current.Ask_Close, current.Price_Close);
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

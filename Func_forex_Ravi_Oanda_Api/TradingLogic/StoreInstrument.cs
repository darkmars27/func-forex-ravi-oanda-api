using Func_forex_Ravi_Oanda_Api.Azure.BlobStorage;
using Func_forex_Ravi_Oanda_Api.Maps;
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
    public class StoreInstrument
    {
        private readonly ILogger<StoreInstrument> log;
        private readonly IOandaApi oandaApi;
        private readonly FxCurrencyTableHelpers tableHelpers;
        public StoreInstrument(ILogger<StoreInstrument> log, IOandaApi oandaApi)
        {
            this.log = log;
            this.oandaApi = oandaApi;
            tableHelpers = new FxCurrencyTableHelpers("oandaforexdatademo");
        }
        public async Task<List<FxCurrencyTable>> Run(string instrument_name)
        {
            try
            {
                var accountInstrumentDetail = await oandaApi.GetAccountInstrumentDetails(instrument_name);
                var prevFxData = tableHelpers.GetPreviousEntities(instrument_name, 50);
                List<FxCurrencyTable> fxdata = new List<FxCurrencyTable>();
                if(prevFxData == null || !prevFxData.Any())
                {
                    // Load History
                    var priceHistory = await oandaApi.GetPriceHistory15Min(instrument_name);
                    fxdata = priceHistory.TransformPricingModelToFxCurrencyTable(accountInstrumentDetail);
                    if (fxdata == null)
                        throw new Exception($"{instrument_name} No History Available");
                }
                else
                {
                    // Get Latest
                    var priceLatest = await oandaApi.GetLatestPrice15Min(instrument_name);
                    fxdata = priceLatest.TransformPricingLatestModelToFxCurrencyTable(accountInstrumentDetail);
                    if (fxdata == null)
                        throw new Exception($"{instrument_name} No Latest Data Available");
                    else
                    {
                        fxdata.AddRange(prevFxData);
                    }
                }

                // Run Indicators
                fxdata = fxdata.OrderByDescending(o => o.RowKey).ToList();
                for (int i = 0; i < fxdata.Count; i++)
                {
                    var current = fxdata[i];
                    FxCurrencyTable previous = null;
                    if(i > 0)
                        previous = fxdata[i - 1];

                    // Calculate SMA 5, 10, 50
                    fxdata[i].SMA_5 = SMAIndicator.CalculateSMA(fxdata, i, 5);
                    fxdata[i].SMA_10 = SMAIndicator.CalculateSMA(fxdata, i, 10);
                    fxdata[i].SMA_50 = SMAIndicator.CalculateSMA(fxdata, i, 50);
                    
                    if(i > 0)
                    {
                        // Calculate EMA 5, 10, 50
                        fxdata[i].EMA_5 = EMAIndicator.CalculateEMA(previous.EMA_5 != 0 ? previous.EMA_5 : previous.SMA_5, fxdata[i].Price_Close, 5);
                        fxdata[i].EMA_10 = EMAIndicator.CalculateEMA(previous.EMA_10 != 0 ? previous.EMA_10 : previous.SMA_10, fxdata[i].Price_Close, 10);
                        fxdata[i].EMA_50 = EMAIndicator.CalculateEMA(previous.EMA_50 != 0 ? previous.EMA_50 : previous.SMA_50, fxdata[i].Price_Close, 50);
                        fxdata[i] = current.DoEMATrendAnalysis(previous);
                    }                    
                }

                await tableHelpers.UpsertEntityAsync(fxdata);
                return fxdata;
            }
            catch (Exception ex) 
            {
                log.LogError(ex.ToString());
                return null;
            }
        }
    }
}

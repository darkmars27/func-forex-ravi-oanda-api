using System.Threading.Tasks;
namespace Ravi.Oanda.Automation
{
    public interface ICurrencies
    {
        public Task<PricingModel> GetLatestPricingWithHistory();
        public void Calculate_EMA(PricingModel pricingHistory);   
        public Task<AccountModel> GetLatestPrice_buy_sell(AccountModel accountDetail);
    }
}
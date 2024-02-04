using Func_forex_Ravi_Oanda_Api;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Func_forex_Ravi_Oanda_Api.Services;
using Func_forex_Ravi_Oanda_Api.Services.Impl;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Func_forex_Ravi_Oanda_Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient("OandaApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OandaApi_BaseUrl"));
                if(!client.DefaultRequestHeaders.Contains("Authorization"))
                    client.DefaultRequestHeaders.Add("Authorization", Environment.GetEnvironmentVariable("OandaApi_DemoKey"));
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddPolicyHandler(GetRetryPolicy());
            builder.Services.AddHttpClient<IOandaApi, OandaApi>().SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            builder.Services.AddSingleton<IOandaApi, OandaApi>();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1));
        }
    }
}

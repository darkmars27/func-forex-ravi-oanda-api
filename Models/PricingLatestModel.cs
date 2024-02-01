namespace OANDA_Automation
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class PricingLatestModel
    {
        [JsonProperty("latestCandles")]
        public List<PricingModel> LatestCandles { get; set; }
    }
}
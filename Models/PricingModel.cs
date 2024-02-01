namespace OANDA_Automation
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class PricingModel
    {
        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        [JsonProperty("granularity")]
        public string Granularity { get; set; }

        [JsonProperty("candles")]
        public List<Candle> Candles { get; set; }
    }

    public partial class Candle
    {
        [JsonProperty("complete")]
        public bool Complete { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("bid")]
        public Ask Bid { get; set; }

        [JsonProperty("mid")]
        public Ask Mid { get; set; }

        [JsonProperty("ask")]
        public Ask Ask { get; set; }
    }

    public partial class Ask
    {
        [JsonProperty("o")]
        public decimal O { get; set; }

        [JsonProperty("h")]
        public decimal H { get; set; }

        [JsonProperty("l")]
        public decimal L { get; set; }

        [JsonProperty("c")]
        public decimal C { get; set; }
    }

  
}

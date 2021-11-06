using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;


namespace Externals.Data
{
    public class AlphaVantageStockResponse
    {
        public Metadata metadata;
        [JsonProperty("Weekly Time Series")]
        public List<TimeSeries> timeseries;
        
    }


    public class Metadata {
        public string Information;
        public string Symbol;
        public string LastRefreshed;
        public string OutputSize;
        public string TimeZone;
    }

    public class TimeSeries
    {
        public string open;
        public string high;
        public string low;
        public string close;
        public string volume;
    }


}

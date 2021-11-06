using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
//using Newtonsoft.Json;
using System.Text.Json;
using Externals.Data;
using AlphaVantage.Net.Common.Intervals;
using AlphaVantage.Net.Common.Size;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks;
using AlphaVantage.Net.Stocks.Client;


namespace Externals
{
    public class AlphaVantageService
    {
        public static async Task<StockTimeSeries> GetRatesForStock(string stockName)
        {
            //string QUERY_URL = "https://www.alphavantage.co/query?function=TIME_SERIES_WEEKLY&symbol=IBM&apikey=demo";
            //Uri queryUri = new Uri(QUERY_URL);

            // use your AlphaVantage API key
            string apiKey = "1";
    // there are 5 more constructors available
            using var client = new AlphaVantageClient(apiKey);
            using var stocksClient = client.Stocks();

            StockTimeSeries stockTs = await stocksClient.GetTimeSeriesAsync(stockName, Interval.Daily, OutputSize.Compact, isAdjusted: true);
            return stockTs;
            //GlobalQuote globalQuote = await stocksClient.GetGlobalQuoteAsync("AAPL");

            //ICollection<SymbolSearchMatch> searchMatches = await stocksClient.SearchSymbolAsync("BA");







            /*using (WebClient client = new WebClient())
            {



                // -------------------------------------------------------------------------
                // if using .NET Core (System.Text.Json)
                // using .NET Core libraries to parse JSON is more complicated. For an informative blog post
                // https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/

                dynamic json_data2 = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(client.DownloadString(queryUri));

                var jsondata3 = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(json_data2["Weekly Time Series"]);

                //dynamic json_data3 = JsonSerializer.Deserialize<string, dynamic>

                var json_data = JsonSerializer.Deserialize<AlphaVantageStockResponse>(client.DownloadString(queryUri));

                return json_data.timeseries;

                // -------------------------------------------------------------------------

                // do something with the json_data
            }*/



        }

    }
}

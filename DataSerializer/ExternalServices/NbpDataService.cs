using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataSerializer.Models;
using DataSerializer.ExternalServices;


namespace DataSerializer
{
    public static class NbpDataService
    {
        private const string BaseUrl = "http://api.nbp.pl/api/exchangerates/rates/a/gbp/2012-01-01/2012-07-31/?format=json";


        public static async Task<List<NbpRate>> GetRatesForCurrencyPair(string currencyPair) {
            string url = $"http://api.nbp.pl/api/exchangerates/rates/a/{currencyPair}/2012-01-01/2012-12-31/?format=json";

            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url)) {
                if (response.IsSuccessStatusCode) {
                    NbpResponse result = JsonConvert.DeserializeObject<NbpResponse>(
                            await response.Content.ReadAsStringAsync());
                    return result.Rates;
                } else {
                    throw new Exception(response.ReasonPhrase);
                }
            }

        }





    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using DataSerializer.Models;
using DataSerializer.ExternalServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DataSerializer
{
    class Program
    {
        private static async Task<List<NbpRate>> LoadFromNbpService(string currencyPair) {
            var r = await NbpDataService.GetRatesForCurrencyPair(currencyPair);

            return r;
        }
        static async Task Main(string[] args)
        {
            ApiHelper.InitClient();
            var res = await LoadFromNbpService("GBP");
            res.ForEach(r => CsvWriter.AddRecord("../../../../MLModels/GPW_GPB.csv", r));

        }


    }
}

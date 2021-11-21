using System;
using Microsoft.Extensions.DependencyInjection;
using DataSerializer.Models;
using DataSerializer.ExternalServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared;
using Externals;

namespace DataSerializer
{
    class Program
    {
        //private static async Task<List<NbpRate>> LoadFromNbpService(string currencyPair) {
        //    var r = await NbpDataService.GetRatesForCurrencyPair(currencyPair);

        //    return r;
        //}
        static async Task Main(string[] args)
        {
            //ApiHelper.InitClient();
            //var res = await LoadFromNbpService("GBP");
            //res.ForEach(r => CsvWriter.AddRecord("../../../../MLModels/GPW_GPB.csv", r));
            var dr = new DataRepository();

            var companyId = dr.getCompanyId("rds-a");
            var point = new StockDataPoint()
            {
                OpeningPrice = 1.23M,
                ClosingPrice = 1.23M,
                HighestPrice = 1.23M,
                LowestPrice = 1.23M,
                Volume = 123,
                Time = new DateTime(),
                CompanyName ="AAAA",
            };
            //dr.PostStockDataPoint(point);

            StoqqService service = new StoqqService();

            var res = service.getGataForBulkLoad("rds-a", companyId);

            dr.PostManyStockDataPoints(res);



        }

 


    }
}

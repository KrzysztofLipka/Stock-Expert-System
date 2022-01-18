using System;
using Microsoft.Extensions.DependencyInjection;
using DataSerializer.ExternalServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared;
using Externals;
using Externals.Data;


namespace DataSerializer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            StoqqService service = new StoqqService();
            DataRepository dataRepository = new DataRepository();
            int companyId; //= dr.getCompanyId("nke");
            string companyName = args[1];
            
            //ApiHelper.InitClient();
            //var res = await LoadFromNbpService("GBP");
            //res.ForEach(r => CsvWriter.AddRecord("../../../../MLModels/GPW_GPB.csv", r));
            if (args[1] != null) {
                companyId = dataRepository.getCompanyId(args[1]);
            } else
            {
                throw new  ArgumentException("Parameter cannot be null");
            }

           

            if (args[0] == "bulkLoad") {
                List<StockDataPointDb> result = new List<StockDataPointDb>();
                result = service.getGataForBulkLoad(companyName, companyId, true);
                //dataRepository.PostManyStockDataPoints(result);
            }

            if (args[0] == "update")
            {
                if (String.IsNullOrEmpty(companyId.ToString()))
                {
                    throw new ArgumentException("Unable to find company data");
                }
                List<StockDataPointDb> result = new List<StockDataPointDb>();
                DateTime updateStartDate = dataRepository.GetLastPriceForCompany(companyName).AddDays(1);
                result = service.GetDataForUpdate(companyName, companyId,updateStartDate);
                //dataRepository.PostManyStockDataPoints(result);
            }

            else
            {
                throw new ArgumentException("Parameter cannot be null");
            }
            //var dr = new DataRepository();

            //DateTime lastPrice = dr.GetLastPriceForCompany("nke").AddDays(1);
            //var nextDayAfterLastPrice = lastPrice.AddDays(1);
            //var companyId = dr.getCompanyId("nke");
            //var res = service.GetDataForUpdate("nke",companyId, nextDayAfterLastPrice);
           

            



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

            

            

            //var res = service.getGataForBulkLoad("^dji", companyId, true);

            //dr.PostManyStockDataPoints(res);



         }

 


    }
}

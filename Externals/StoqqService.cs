using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Shared;
using Externals.Mappers;
using Externals.Data;


namespace Externals
{
    public class StoqqService
    {
        //private static string url = "https://stooq.pl/q/d/l/?s=aapl.us&i=d";
        public List<StockDataPointDb> getGataForBulkLoad(string companyName, int companyId) {

            string url = $"https://stooq.pl/q/d/l/?s={companyName}.us&i=d";
            //https://stooq.pl/q/d/l/?s=rds-a.us&i=d
            //https://stooq.pl/q/d/l/?s=aapl.us&d1=20210907&d2=20211117&i=d
            CsvHelper helper = new CsvHelper();
            var stoqqResponse = helper.GetCSV(url);

            List<StockDataPointDb> result = new List<StockDataPointDb>();
            //string fileList = getCSV("http://www.google.com");
            string[] tempStr;

            tempStr = stoqqResponse.Split("\r\n");

            foreach (string item in tempStr)
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Equals("Data,Otwarcie,Najwyzszy,Najnizszy,Zamkniecie,Wolumen"))
                {
                    var split = StoqqDataPointsToDataPointsMapper.Map(item.Split(','),companyId) ;
                    result.Add(split);
                }
            }

            return result;
        }

        /*public List<StockDataPoint> getDataForTimeRange(string companyName, DateTime startDate, DateTime endDate) {
            //20210907
            string startDateAsString = startDate.ToString("yyyyMMdd");
            string endDateAsString = startDate.ToString("yyyyMMdd");

            string url = $"https://stooq.pl/q/d/l/?s={companyName}.us&d1={startDateAsString}&d2={endDateAsString}&i=d";

            CsvHelper helper = new CsvHelper();
            var stoqqResponse = helper.GetCSV(url);

            List<StockDataPoint> result = new List<StockDataPoint>();

            string[] tempStr = stoqqResponse.Split("\r\n");

            foreach (string item in tempStr)
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Equals("Data,Otwarcie,Najwyzszy,Najnizszy,Zamkniecie,Wolumen"))
                {
                    var split = StoqqDataPointsToDataPointsMapper.Map(item.Split(','), companyId);
                    result.Add(split);
                }
            }

            return result;


        }*/

       

    }
}

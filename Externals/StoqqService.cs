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
        public List<StockDataPointDb> getGataForBulkLoad(string companyName, int companyId,bool isIndex = false) {

            string url = !isIndex 
                ? $"https://stooq.pl/q/d/l/?s={companyName}.us&i=d"
                : $"https://stooq.pl/q/d/l/?s={companyName}&i=d"
            ;
            //https://stooq.pl/q/d/l/?s=rds-a.us&i=d
            //https://stooq.pl/q/d/l/?s=aapl.us&d1=20210907&d2=20211117&i=d
            //https://stooq.pl/q/d/l/?s=^ndq&i=d
            CsvHelper helper = new CsvHelper();
            var stoqqResponse = helper.GetCSV(url);

            return ProcessStoqqCsvFile(stoqqResponse, companyId, isIndex);

            /*List<StockDataPointDb> result = new List<StockDataPointDb>();
            //string fileList = getCSV("http://www.google.com");
            string[] tempStr;

            tempStr = stoqqResponse.Split("\r\n");

            foreach (string item in tempStr)
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Equals("Data,Otwarcie,Najwyzszy,Najnizszy,Zamkniecie,Wolumen"))
                {
                    //todo
                    var split = StoqqDataPointsToDataPointsMapper.Map(item.Split(','),companyId, isIndex) ;
                    result.Add(split);
                }
            }

            return result;*/
        }

        public List<StockDataPointDb> GetDataForUpdate(string companyName, int companyId, DateTime lastUpdateDate, bool isIndex = false) {
            //string url = !isIndex
            //   ? $"https://stooq.pl/q/d/l/?s={companyName}.us&i=d"
            //   : $"https://stooq.pl/q/d/l/?s={companyName}&i=d";
            string url = !isIndex
                ? $"https://stooq.pl/q/d/l/?s={companyName}.us&d1={lastUpdateDate.ToString("yyyyMMdd")}" +
                $"&d2={DateTime.Today.ToString("yyyyMMdd")}&i=d"
                : $"https://stooq.pl/q/d/l/?s={companyName}&d1={lastUpdateDate.ToString("yyyyMMdd")}" +
                $"&d2={DateTime.Today.ToString("yyyyMMdd")}&i=d";

            CsvHelper helper = new CsvHelper();
            var stoqqResponse = helper.GetCSV(url);

            return ProcessStoqqCsvFile(stoqqResponse, companyId, isIndex);


            //"MM-dd-yy"
        }

        private List<StockDataPointDb> ProcessStoqqCsvFile(string stoqqResponse,int companyId,bool isIndex) {
            List<StockDataPointDb> result = new List<StockDataPointDb>();
            //string fileList = getCSV("http://www.google.com");
            string[] tempStr;

            tempStr = stoqqResponse.Split("\r\n");

            foreach (string item in tempStr)
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.Equals("Data,Otwarcie,Najwyzszy,Najnizszy,Zamkniecie,Wolumen"))
                {
                    //todo
                    var split = StoqqDataPointsToDataPointsMapper.Map(item.Split(','), companyId, isIndex);
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

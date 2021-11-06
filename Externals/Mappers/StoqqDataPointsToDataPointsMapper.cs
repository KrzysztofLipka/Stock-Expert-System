using System;
using System.Collections.Generic;
using System.Text;
using Shared;
using System.Globalization;
using Externals.Data;

namespace Externals.Mappers
{
    public class StoqqDataPointsToDataPointsMapper
    {
        //Data,Otwarcie,Najwyzszy,Najnizszy,Zamkniecie,Wolumen

        public static StockDataPointDb Map(string[] dataRowsSplitted, int companyId) {
            //for (int i = 0; i < dataRowsSplitted.Length; i++) {

            //}
            //const string format = "YYYY-MM-DD";

            var info = new CultureInfo("en-US");
            return new StockDataPointDb()
            {
                Time = DateTime.ParseExact(dataRowsSplitted[0], "yyyy-MM-dd", null),
                OpeningPrice = Convert.ToDecimal(dataRowsSplitted[1],info),
                HighestPrice = Convert.ToDecimal(dataRowsSplitted[2],info),
                LowestPrice = Convert.ToDecimal(dataRowsSplitted[3],info),
                ClosingPrice = Convert.ToDecimal(dataRowsSplitted[4], info),
                Volume = Convert.ToInt64(dataRowsSplitted[5]),
                CompanyId = companyId
            };
        }
    }
}

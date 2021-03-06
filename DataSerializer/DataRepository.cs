using System.Configuration;
using System.Collections.Specialized;

using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using System.Data;
using Externals.Data;


namespace DataSerializer
{
    public class DataRepository : IDataRepository
    {
        private string connectionString;
        public DataRepository()
        {
            //connectionString = ConfigurationManager.AppSettings.Get("connectionString");
            connectionString = "";
        }
       

        public IEnumerable<StockDataPoint> GetStockDataPoints(string StockName)
        {
            throw new NotImplementedException();
        }

        public void PostManyStockDataPoints(IEnumerable<StockDataPointDb> dataPoint)
        {
            //throw new NotImplementedException();
            var dt = new DataTable();
            //var companyId = new DataColumn();
            //companyId.DataType = Type.GetType("System.Int32");
            //dt.Columns.Add(companyId);
            dt.Columns.Add("PriceId",typeof(int));
            dt.Columns.Add("CompanyId", typeof(int));         
            dt.Columns.Add("OpeningPrice", typeof(float));
            dt.Columns.Add("ClosingPrice", typeof(float));
            dt.Columns.Add("HighestPrice", typeof(float));
            dt.Columns.Add("LowestPrice", typeof(float));
            dt.Columns.Add("Volume", typeof(long));
            dt.Columns.Add("PriceDate", typeof(DateTime));

            foreach (var point in dataPoint) {
                dt.Rows.Add(
                    0,
                    point.CompanyId,
                    (float)point.OpeningPrice,
                    (float)point.ClosingPrice,
                    (float)point.HighestPrice,
                    (float)point.LowestPrice,
                    point.Volume,
                    point.Time
                    );
            }

            var l = dt;


            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName =
                    "dbo.HistoricalPrices";

                try
                {
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void PostStockDataPoint(StockDataPoint dataPoint)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(

                    @"EXEC dbo.Add_Price
                @CompanyName = @CompanyName, 
                @OpeningPrice = @OpeningPrice, 
                @ClosingPrice = @ClosingPrice, 
                @HighestPrice = @HighestPrice, 
                @LowestPrice = @LowestPrice, 
                @Volume = @Volume", dataPoint);       
            }
        }

        public  int PostPrediction(HistoricalPrediction prediction)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var predictionId = connection.QueryFirst<int>(

                @"EXEC dbo.Add_Prediction
                @CompanyName = @CompanyName, 
                @PredictedBuyPrice = @PredictedBuyPrice, 
                @PredictedSellPrice = @PredictedSellPrice, 
                @ActualBuyPrice = @ActualBuyPrice, 
                @ActualSellPrice = @ActualSellPrice, 
                @StartDate = @StartDate,
                @EndDate = @EndDate",
                prediction);

                return predictionId;

            }

           

            //public decimal OpeningPrice { get; set; }
            //public decimal ClosingPrice { get; set; }
            //public decimal HighestPrice { get; set; }
            //public decimal LowestPrice { get; set; }
            //public long Volume { get; set; }
            //public DateTime Time { get; set; }

            //public string StockName { get; set; }

        }

        public void CleanPredictions() {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(

                @"delete from dbo.Predictions;");

               

            }

        }

        public int getCompanyId(string companyName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var result = 
                connection.QueryFirst<int>(

                @"EXEC dbo.GetCompanyId
                @CompanyName = @CompanyName", new {CompanyName = companyName });

                return result;


            }

        }

        public DateTime GetLastPriceForCompany (string companyName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var result =
                connection.QueryFirst<DateTime>(

                @"EXEC dbo.GetLastPriceForCompany
                @CompanyName = @CompanyName", new { CompanyName = companyName });

                return result;


            }

        }

    }
}

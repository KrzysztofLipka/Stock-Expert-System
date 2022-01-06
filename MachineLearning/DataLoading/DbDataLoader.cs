using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MachineLearning.DataLoading
{
    public class DbDataLoader
    {
        private static readonly string connectionString = 
            "";
        public static IDataView LoadDataFromDb(
            MLContext context, string companyName, 
            out int numberOfRows, 
            out DateTime lastUpdated, 
            int numberOfRowsToLoad = 0)
        {
            //DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            string sqlCommand2 = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '03-01-2021'";
            string sqlCommand3 = $"EXEC dbo.[GetLastClosingPrices] @CompanyName = '{companyName}', @NumberOfLastRows = {numberOfRowsToLoad}";

            // DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);
            //IDataView dataFromDb = loader.Load(dbSource);
            //IEnumerable<StockDataPointInput> data = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false);
            var data = FetchData(context, sqlCommand);

            if (numberOfRowsToLoad != 0) {
                data = data.Skip(Math.Max(0, data.Count() - numberOfRowsToLoad));
            }

            lastUpdated = data.Last().Date;
            numberOfRows = data.Count();

            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);
        }

        public static IDataView LoadDataFromDb(
            MLContext context, string companyName, 
            out int numberOfRows,  
            DateTime maxDate, int numberOfRowsToLoad = 0)
        {
            //DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            //todo validate tostring
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '{maxDate.ToString("dd-MM-yy")}'";
            
            //DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);

            //IDataView dataFromDb = loader.Load(dbSource);

            //numberOfRows = dataFromDb.Preview(20000).RowView.Length;
            //var data1 = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var data = FetchData(context, sqlCommand);

            if (numberOfRowsToLoad != 0)
            {
                data = data.Skip(Math.Max(0, data.Count() - numberOfRowsToLoad));
            }

            numberOfRows = data.Count();
            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);

        }

        public static IDataView LoadDataFromDb(
           MLContext context, string companyName,
           out int numberOfRows, DateTime minDate,
           DateTime maxDate)
        {
            //string sqlCommand4 = $"EXEC dbo.GetClosingPricesWithMinMaxDate @CompanyName = '{companyName}', @MaxDate = '03-05-2016', @MinDate = '03-05-2021'";
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMinMaxDate " +
                $"@CompanyName = '{companyName}', " +
                $"@MinDate = '{minDate.ToString("dd-MM-yy")}' , " +
                $"@MaxDate = '{maxDate.ToString("dd-MM-yy")}'";
            var data = FetchData(context, sqlCommand);

            numberOfRows = data.Count();
            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);

        }

        private static IEnumerable<StockDataPointInput> FetchData(MLContext context, string sqlCommand) {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);
            IDataView dataFromDb = loader.Load(dbSource);
            return context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false);
        }
    }
}

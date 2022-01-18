using System.Configuration;
using System.Collections.Specialized;
using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using Shared;


namespace MachineLearning.DataLoading
{
    public class DbDataLoader
    {
        private string connectionString;

        public DbDataLoader()
        {
            connectionString =
                "";
            //ConfigurationManager.AppSettings.Get("connectionString");
        }
          
        public IDataView LoadDataFromDb(
            MLContext context, string companyName, 
            out int numberOfRows, 
            out DateTime lastUpdated, 
            int numberOfRowsToLoad = 0)
        {
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            string sqlCommand2 = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '03-01-2021'";
            string sqlCommand3 = $"EXEC dbo.[GetLastClosingPrices] @CompanyName = '{companyName}', @NumberOfLastRows = {numberOfRowsToLoad}";
            var data = FetchData(context, sqlCommand);

            if (numberOfRowsToLoad != 0) {
                data = data.Skip(Math.Max(0, data.Count() - numberOfRowsToLoad));
            }

            lastUpdated = data.Last().Date;
            numberOfRows = data.Count();

            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);
        }

        public IEnumerable<StockDataPointInput> LoadDataFromDb(
            string companyName,
            out int numberOfRows,
            int numberOfRowsToLoad = 0
            )
        {
            //for arima
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            MLContext context = new MLContext();

            IEnumerable<StockDataPointInput> result =  FetchData(context, sqlCommand);
            numberOfRows = result.Count();
            if (numberOfRowsToLoad != 0)
            {
                result = result.Skip(Math.Max(0, result.Count() - numberOfRowsToLoad));
            }
            return result;
            
        }

        public IEnumerable<StockDataPointInput> LoadDataFromDb(
            string companyName,
            out int numberOfRows,
            DateTime maxDate,
            int numberOfRowsToLoad = 0
            )
        {
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMaxDate " +
                $"@CompanyName = '{companyName}', " +
                $"@MaxDate = '{maxDate.ToString("MM-dd-yy")}'";
            MLContext context = new MLContext();

            IEnumerable<StockDataPointInput> result = FetchData(context, sqlCommand);
            numberOfRows = result.Count();
            if (numberOfRowsToLoad != 0)
            {
                result = result.Skip(Math.Max(0, result.Count() - numberOfRowsToLoad));
            }
            return result;

        }

        public IEnumerable<StockDataPointInput> LoadDataFromDbFromMinDate(
            string companyName,
            
            DateTime minDate,
            int numberOfRowsToTake = 0
            )
        {
            var dat = minDate.ToString("MM-dd-yy");
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMinDate " +
                $"@CompanyName = '{companyName}', " +
                $"@MinDate = '{minDate.ToString("MM-dd-yy")}'";
            MLContext context = new MLContext();

            IEnumerable<StockDataPointInput> result = FetchData(context, sqlCommand);
            
            if (numberOfRowsToTake != 0)
            {
                result = result.Take(numberOfRowsToTake);
            }
            return result;

        }


        public IEnumerable<StockDataPointInput> LoadDataFromDb(
            string companyName,
            out int numberOfRows,
            DateTime minDate,
            DateTime maxDate
            )
        {
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMinMaxDate " +
                $"@CompanyName = '{companyName}', " +
                $"@MinDate = '{minDate.ToString("MM-dd-yy")}' , " +
                $"@MaxDate = '{maxDate.ToString("MM-dd-yy")}'";
            MLContext context = new MLContext();

            IEnumerable<StockDataPointInput> result = FetchData(context, sqlCommand);
            numberOfRows = result.Count();
            return result;

        }

        public IDataView LoadDataFromDb(
            MLContext context, string companyName, 
            out int numberOfRows,
            out DateTime lastUpdated,
            DateTime maxDate, int numberOfRowsToLoad = 0)
        {
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '{maxDate.ToString("MM-dd-yy")}'";
            
            var data = FetchData(context, sqlCommand);

            if (numberOfRowsToLoad != 0)
            {
                data = data.Skip(Math.Max(0, data.Count() - numberOfRowsToLoad));
            }

            lastUpdated = data.Last().Date;

            numberOfRows = data.Count();
            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);

        }

        public IDataView LoadDataFromDb(
           MLContext context, string companyName,
           out int numberOfRows, DateTime minDate,
           DateTime maxDate)
        {
            string sqlCommand = $"EXEC dbo.GetClosingPricesWithMinMaxDate " +
                $"@CompanyName = '{companyName}', " +
                $"@MinDate = '{minDate.ToString("MM-dd-yy")}' , " +
                $"@MaxDate = '{maxDate.ToString("MM-dd-yy")}'";
            var data = FetchData(context, sqlCommand);

            numberOfRows = data.Count();
            return context.Data.LoadFromEnumerable<StockDataPointInput>(data);

        }

        public IEnumerable<HistoricalPrediction> LoadHistoricalPredictions()
        {
            var context = new MLContext();
            string sqlCommand = @"SELECT [PredictionId]
                                , d.CompanyName
                                ,[PredictedBuyPrice]
                                ,[PredictedSellPrice]
                                ,[ActualBuyPrice]
                                ,[ActualSellPrice]
                                ,[StartDate]
                                ,[EndDate]
                                FROM[ExpertSystem].[dbo].[Predictions] p INNER JOIN dbo.Companies d ON p.CompanyId = d.CompanyId; ";
            var data = FetchHistoricalPredictionData(context, sqlCommand);

            //numberOfRows = data.Count();
            return data;

        }

        private IEnumerable<StockDataPointInput> FetchData(MLContext context, string sqlCommand) {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);
            IDataView dataFromDb = loader.Load(dbSource);
            return context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false);
        }

        private IEnumerable<HistoricalPrediction> FetchHistoricalPredictionData(MLContext context, string sqlCommand)
        {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<HistoricalPrediction>();
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);
            IDataView dataFromDb = loader.Load(dbSource);
            return context.Data.CreateEnumerable<HistoricalPrediction>(dataFromDb, reuseRowObject: false);
        }
    }
}

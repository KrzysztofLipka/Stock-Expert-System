using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MachineLearning.Trainers
{
    public class ForecastBySsa
    {

        string connectionString = "todo move to config";
        public void Predict(string dataPath, string modelOuptutPath, string companyName) {


        
            var context = new MLContext();

            //----------------

            //var columns = new DatabaseLoader.Column[] {
            //        new DatabaseLoader.Column() {Name="ClosingPrice" , Type = System.Data.DbType.Single }
            //};

            //DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            ////string sqlCommand = "select ClosingPrice from dbo.HistoricalPrices where CompanyId = 1 order by PriceId";
            //string sqlCommand =  $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";





            //DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);

            IDataView dataFromDb = LoadDataFromDb(context, companyName);

            //------------------

            var data = context.Data.LoadFromTextFile<NbpData>(dataPath,
                hasHeader: false,
                separatorChar: ',');
            var lastElementInFile = data.Preview(100000).RowView.Last();
           

            Console.WriteLine(data.Preview().RowView);
            Console.WriteLine(nameof(NbpData.ExchangeRate));

            var pipeline = context.Forecasting.ForecastBySsa(
                "Forecast",
               "ClosingPrice",
               windowSize: 5,
               seriesLength: 10,
               trainSize: 215,
               horizon: 4);

            var model = pipeline.Fit(dataFromDb);

            var forecastingEngine = model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(context);

            var forecasts = forecastingEngine.Predict();

            foreach (var forecast in forecasts.Forecast)
            {
                Console.WriteLine(forecast);
            }

            forecastingEngine.CheckPoint(context, modelOuptutPath);

            var test = new List<StockDataPointInput>() {
                new StockDataPointInput(){
                    //Date= "11",
                    ClosingPrice= 5.034033F
                }, new StockDataPointInput(){
                    //Date= "11",
                    ClosingPrice= 5.002082F
                }, new StockDataPointInput(){
                    //Date= "11",
                    ClosingPrice= 4.9799623F
                },new StockDataPointInput(){
                    //Date= "11",
                    ClosingPrice= 4.9721212F
                }
            };

            var testData = context.Data.LoadFromEnumerable<StockDataPointInput>(test);

            this.Evaluate(testData, model, context);

            Console.ReadLine();
        }

        public void UpdateModel(NbpData[] updateData, string modelOuptutPath) {
            var context = new MLContext();
            ITransformer model;
            using (var file = File.OpenRead("model.zip"))
                model = context.Model.Load(file, out DataViewSchema schema);

            var forecastingEngine = model.CreateTimeSeriesEngine<NbpData, NbpForecastOutput>(context);

            foreach (var data in updateData) {
                forecastingEngine.Predict(data);
            }

            var forecasts = forecastingEngine.Predict();

            foreach (var forecast in forecasts.Forecast)
            {
                Console.WriteLine(forecast);
            }

            forecastingEngine.CheckPoint(context, modelOuptutPath);
        }

        void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<float> actual =
            mlContext.Data.CreateEnumerable<StockDataPointInput>(testData, true)
        .   Select(observed => observed.ClosingPrice);

            IEnumerable<float> forecast = mlContext.Data.CreateEnumerable<NbpForecastOutput>(predictions, true)
                .Select(prediction => prediction.Forecast[0]);
            Console.WriteLine("wwwwwwwwwwwww");
            Console.WriteLine(actual.ToArray()[0]);
            Console.WriteLine(actual.ToArray()[1]);
            Console.WriteLine(actual.ToArray()[2]);
            Console.WriteLine(actual.ToArray()[3]);
            Console.WriteLine("wwwwwwwwwwwww");
            Console.WriteLine(forecast.ToArray()[0]);
            Console.WriteLine(forecast.ToArray()[1]);
            Console.WriteLine(forecast.ToArray()[2]);
            Console.WriteLine(forecast.ToArray()[3]);


            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            Console.WriteLine("Evaluation Metrics");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
            Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");

        }

        private IDataView LoadDataFromDb(MLContext context, string companyName) {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            //string sqlCommand = "select ClosingPrice from dbo.HistoricalPrices where CompanyId = 1 order by PriceId";
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";

            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);

            IDataView dataFromDb = loader.Load(dbSource);

            return dataFromDb;
        }

    }

}

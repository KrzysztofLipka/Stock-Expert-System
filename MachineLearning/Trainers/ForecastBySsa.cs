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
using ScottPlot;

namespace MachineLearning.Trainers
{
    public class ForecastBySsa
    {

        string connectionString = "";
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

            int numberOfRows;
            IDataView dataFromDb = LoadDataFromDb(context, companyName,out numberOfRows);

            

            Console.WriteLine(numberOfRows);

            //double testFraction = Math.Round(365 /(double)numberOfRows,4);

            

            //double test2 = numberOfRows * testFraction;

            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var splitIndex = numberOfRows - 365;

            List<StockDataPointInput> trainList;
            List<StockDataPointInput> testList;

            SplitList(dataFromDbToArray, out trainList, out testList, splitIndex);

            //var testData = context.Data.LoadFromEnumerable<StockDataPointInput>(test);

            var trainSet = context.Data.LoadFromEnumerable<StockDataPointInput>(trainList); 

            var testSet = context.Data.LoadFromEnumerable<StockDataPointInput>(testList);




            //var split = context.Data
            //   .TrainTestSplit(dataFromDb, testFraction: testFraction
            //   );

            //var trainSet = context.Data
            //   .CreateEnumerable<StockDataPointInput>(split.TrainSet, reuseRowObject: false);


            //var testSet = context.Data
            //    .CreateEnumerable<StockDataPointInput>(split.TestSet, reuseRowObject: false);

            //------------------

            //var data = context.Data.LoadFromTextFile<NbpData>(dataPath,
            //    hasHeader: false,
            //    separatorChar: ',');
            //var lastElementInFile = data.Preview(100000).RowView.Last();


            //Console.WriteLine(data.Preview().RowView);
            //Console.WriteLine(nameof(NbpData.ExchangeRate));

            var pipeline = context.Forecasting.ForecastBySsa(
                "Forecast",
               "ClosingPrice",
               windowSize: trainList.Count/4,
               seriesLength: trainList.Count,
               trainSize: trainList.Count,
               horizon: 365);

            //var model = pipeline.Fit(dataFromDb);
            var model = pipeline.Fit(trainSet);


            var forecastingEngine = model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(context);

            var forecasts = forecastingEngine.Predict();

            foreach (var forecast in forecasts.Forecast)
            {
                Console.WriteLine(forecast);
            }

            var resAsList = forecasts.Forecast;

            double[] forecastsArray = Array.ConvertAll(resAsList, x => (double)x);

            double[] xs = new double[] { 1, 2, 3, 4, 5 };
            double[] ys = new double[] { 1, 4, 9, 16, 25 };

            double [] y = DataGen.Consecutive(365);
            double [] x1 = ConvertInputListToPlotArray(testList);

            var plt = new ScottPlot.Plot(400, 300);
            plt.AddScatter(y, x1);
            plt.AddScatter(y, forecastsArray);
            plt.SaveFig("console.png");

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

            this.Evaluate(testSet, model, context);

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

        private IDataView LoadDataFromDb(MLContext context, string companyName, out int numberOfRows) {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            //string sqlCommand = "select ClosingPrice from dbo.HistoricalPrices where CompanyId = 1 order by PriceId";
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            string sqlCommand2 = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '11-11-2019'";

            //EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = 'AAPL', @MaxDate = '11-11-2020'

            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand2);

            IDataView dataFromDb = loader.Load(dbSource);

            numberOfRows = dataFromDb.Preview(20000).RowView.Length;

            return dataFromDb;
        }

        private void SplitList(
            List<StockDataPointInput> inputList,
            out List<StockDataPointInput> listOne,
            out List<StockDataPointInput> listTwo,
            int splitIndex
            )
        {
            var res1 = new List<StockDataPointInput>();
            var res2 = new List<StockDataPointInput>();

            for (int i = 0; i < inputList.Count; i++)
            {
                if (i < splitIndex) {
                    res1.Add(inputList[i]);
                } else {
                    res2.Add(inputList[i]);
                };
                    
            }

            listOne = res1;
            listTwo = res2;
        }

        private double[] ConvertInputListToPlotArray(List<StockDataPointInput> list) {

            var res = new List<double>();
            foreach (var item in list)
            {
                res.Add((double)item.ClosingPrice);
            }

            return res.ToArray();
            
        }

    }

}

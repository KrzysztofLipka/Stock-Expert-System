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
using Microsoft.ML.TimeSeries;
using MathNet.Numerics;
using System.Drawing;

namespace MachineLearning.Trainers
{

    public class ForecastBySsa
    {
        string connectionString = "";
        public void Predict(string dataPath, string modelOuptutPath, string companyName, ForecastBySsaParams parameters, int numberOfRowsToLoad, out double mae, out double rmse, out double acf) {

            int horizon = parameters.Horizon;
            var context = new MLContext();
            int numberOfRows;
            DateTime lastUpdate;
            IDataView dataFromDb = LoadDataFromDb(context, companyName,out numberOfRows, numberOfRowsToLoad, out lastUpdate);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var splitIndex = numberOfRows - horizon;

            List<StockDataPointInput> trainList;
            List<StockDataPointInput> testList;

            SplitList(dataFromDbToArray, out trainList, out testList, splitIndex);

            var trainSet = context.Data.LoadFromEnumerable<StockDataPointInput>(trainList); 

            var testSet = context.Data.LoadFromEnumerable<StockDataPointInput>(testList);

            //var seriesLength = horizon * 2;

            //var windowSize = seriesLength / 3;

            //var t = parameters.TrainSize / 2;
            var seriesLength = trainList.Count;
            var windowSize = seriesLength/3;
            var trainSize = trainList.Count;
            var hor = 30;



            var pipeline = context.Forecasting.ForecastBySsa(
                "Forecast",
               "ClosingPrice",
               windowSize: windowSize,
               seriesLength: seriesLength,
               trainSize: trainSize,
               horizon: 30
               
               //maxRank: 1
               //shouldStabilize: false


               );

           
            //var model = pipeline.Fit(dataFromDb);
            var model = pipeline.Fit(trainSet);

            string plotLabel = $"windowsize: {windowSize}, " +
                $"seriesLength: {seriesLength}, trainSize: {trainSize}, " +
                $"horizon: {horizon}, całkowita ilośc danych: {numberOfRowsToLoad}";

            double Mae;
            double Rmse;
            double Acf;
            this.Evaluate(testSet, model, context, plotLabel, out Mae, out Rmse, out Acf);

            mae = Mae;
            rmse = Rmse;
            acf = Acf;


            var forecastingEngine = model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(context);

            foreach (var forecast in testList)
            {
                forecastingEngine.Predict(forecast);
            }

            forecastingEngine.CheckPoint(context, modelOuptutPath + $"{lastUpdate.ToShortDateString()}.zip");



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

        void Evaluate(IDataView testData, ITransformer model, MLContext mlContext, string label, out double MAE, out double RMSE, out double ACF)
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<float> actual =
            mlContext.Data.CreateEnumerable<StockDataPointInput>(testData, true)
        .   Select(observed => observed.ClosingPrice);

            IEnumerable<float> forecast = mlContext.Data.CreateEnumerable<NbpForecastOutput>(predictions, true)
                .Select(prediction => prediction.Forecast[0]);
           
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

           MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
           RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
           ACF = MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(forecast, 2, x => x * x);

           PlotData(actual.Count(), actual, forecast, label,MAE, RMSE,ACF);

           Console.WriteLine("Evaluation Metrics");
           Console.WriteLine("---------------------");
           Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
           Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
           Console.WriteLine($"Auto Correlation Function: {ACF:F3}\n");

        }

        private IDataView LoadDataFromDb(MLContext context, string companyName, out int numberOfRows, int numberOfRowsToLoad,out DateTime lastUpdated, DateTime? requestedUpdateDate = null) {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            //string sqlCommand = "select ClosingPrice from dbo.HistoricalPrices where CompanyId = 1 order by PriceId";
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            string sqlCommand2 = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '03-01-2021'";
            string sqlCommand3 = $"EXEC dbo.[GetLastClosingPrices] @CompanyName = '{companyName}', @NumberOfLastRows = {numberOfRowsToLoad}";

            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand);

            IDataView dataFromDb = loader.Load(dbSource);

            //numberOfRows = dataFromDb.Preview(20000).RowView.Length;
            var data1 = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var data2 =  data1.Skip(Math.Max(0, data1.Count() - numberOfRowsToLoad));

            lastUpdated = data2.Last().Date;

            numberOfRows = data2.Count();

            var data3 = context.Data.LoadFromEnumerable<StockDataPointInput>(data2);

            return data3;
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

        private void PlotData(int horizon, IEnumerable<float>testList, IEnumerable<float> forecastsList, string label, double mae, double rmse, double acf) {
            double[] y = DataGen.Consecutive(horizon);
            double[] testArray = Array.ConvertAll(testList.ToArray(), x => (double)x);
            double[] forecastsArray = Array.ConvertAll(forecastsList.ToArray(), x => (double)x);

            var plt = new ScottPlot.Plot(1500, 500);
            plt.AddSignal(testArray, horizon);

            plt.AddSignal(forecastsArray, horizon);
            plt.AddAnnotation($"MAE: {mae:F3} \nRMSE: {rmse:F3} \nACF: {acf:F3}", 10, -10);

            plt.XAxis.Label(label);
            plt.SaveFig("console.png");

        }

    }

}

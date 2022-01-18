using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using ScottPlot;
using MachineLearning.DataLoading;

namespace MachineLearning.Trainers
{
    public abstract class SSATrainerBase
    {
        public abstract SsaForecastOutput Forcast(
            string companyName, 
            int horizon, 
            DateTime maxDate = new DateTime());

        public abstract SsaForecastOutput Forcast(
           string dataPath,
           string modelOuptutPath,
           string companyName,
           ForecastBySsaParams parameters,
           int numberOfRowsToLoad,
           bool saveModel = false,
           DateTime maxDate = new DateTime(),
           bool plotResult = false,
           string finalPlotName = "plot"
           );


        protected SSASolveResult Solve(
            MLContext context, List<StockDataPointInput> dataFromDbToArray,
            int numberOfRows, int horizon,
            int defaultWindowSize, int trainSize,
            int seriesLength, bool plotData = false,
            string plotFileName = "plot"
            )
        {
            var splitIndex = (int)(numberOfRows * 0.8);
            //var splitIndex = numberOfRows - horizon*2;

            IDataView trainSet; //= context.Data.LoadFromEnumerable<StockDataPointInput>(trainList); 
            IDataView testSet; //= context.Data.LoadFromEnumerable<StockDataPointInput>(testList);

            //int trainListSize;

            SplitData(context, dataFromDbToArray, out trainSet, out testSet, splitIndex);

            SsaForecastingEstimator pipeline = context.Forecasting.ForecastBySsa(
                  "Forecast",
                 "ClosingPrice",
                 windowSize: defaultWindowSize, //windowSize,
                 seriesLength: seriesLength,//windowSize*2, //seriesLength,
                 trainSize: trainSize,
                 horizon: horizon);

            SsaForecastingTransformer model = pipeline.Fit(trainSet);


            string plotLabel = $"windowsize: {defaultWindowSize}, " +
                    $"seriesLength: { seriesLength}, trainSize: {trainSize}, " +
                    $"horizon: {horizon}, całkowita ilośc danych: {numberOfRows}";

            Evaluate(testSet, model, context, plotLabel, out double Mae, out double Rmse, out double Acf, plotData, plotFileName);

            return new SSASolveResult()
            {
                Acf = Acf,
                Rmse = Rmse,
                Mae = Mae,
                //WindowSize = defaultWindowSize,
                //BestWindowSize = BestWindow,
                Model = model
            };
        }


        public SSASolveResult SolveAndPlot(
            DateTime maxDate, string companyName, int numberOfRowsToLoad,
            int horizon,
            int defaultWindowSize, int trainSize,
            int seriesLength,
            string plotFileName
            )
        {
            MLContext context = new MLContext();
            DateTime lastUpdate;
            int numberOfRows;
            var dataLoader = new DbDataLoader();

            IDataView dataFromDb = maxDate < DateTime.Today
               ? dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, maxDate, numberOfRowsToLoad)
               : dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            return Solve(context, dataFromDbToArray, numberOfRows, horizon, defaultWindowSize, trainSize, seriesLength, true, plotFileName);
        }


        protected void SplitData(
            MLContext context, List<StockDataPointInput> data,
            out IDataView trainSet, out IDataView testSet,
            int splitIndex)
        {

            List<StockDataPointInput> trainList;
            List<StockDataPointInput> testList;

            //todo use generic method
            SplitList(data, out trainList, out testList, splitIndex);
            trainSet = context.Data.LoadFromEnumerable<StockDataPointInput>(trainList);
            testSet = context.Data.LoadFromEnumerable<StockDataPointInput>(testList);
            //trainListSize = trainList.Count();
        }

        //todo
        protected void SplitList(
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
                if (i < splitIndex)
                {
                    res1.Add(inputList[i]);
                }
                else
                {
                    res2.Add(inputList[i]);
                };

            }

            listOne = res1;
            listTwo = res2;
        }

        protected void Evaluate(IDataView testData, ITransformer model, MLContext mlContext, string label, out double MAE, out double RMSE, out double ACF, bool plotData = false, string plotFileNme = "plot")
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<StockDataPointInput> actualPoints = mlContext.Data.CreateEnumerable<StockDataPointInput>(testData, true);

            IEnumerable<float> actual = actualPoints.Select(observed => observed.ClosingPrice);
            IEnumerable<DateTime> actualDates = actualPoints.Select(observed => observed.Date);

            IEnumerable<float> forecast = mlContext.Data.CreateEnumerable<ForecastOutput>(predictions, true)
                .Select(prediction => prediction.Forecast[0]);

            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
            ACF = MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(forecast, 2, x => x * x);

            if (plotData)
            {
                PlotData(actual.Count(), actual, forecast, label, MAE, RMSE, ACF, plotFileNme, actualDates);
            }


            Console.WriteLine("Evaluation Metrics");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
            Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
            Console.WriteLine($"Auto Correlation Function: {ACF:F3}\n");

        }


        protected void PlotData(
            int horizon, IEnumerable<float> testList,
            IEnumerable<float> forecastsList, string label,
            double mae, double rmse,
            double acf, string plotFileName,
            IEnumerable<DateTime> dates = null
            )
        {
            double[] y = dates != null
                ? dates.Select(date => date.ToOADate()).ToArray()
                : DataGen.Consecutive(horizon);
            double[] testArray = Array.ConvertAll(testList.ToArray(), x => (double)x);
            double[] forecastsArray = Array.ConvertAll(forecastsList.ToArray(), x => (double)x);

            var plt = new ScottPlot.Plot(1500, 500);
            //plt.AddSignal(testArray, horizon);

            //plt.AddSignal(forecastsArray, horizon);

            plt.AddScatter(y, testArray);

            plt.AddScatter(y, forecastsArray);



            plt.AddAnnotation($"MAE: {mae:F3} \nRMSE: {rmse:F3} ", 10, -10);

            plt.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Day);
            plt.XAxis.TickLabelStyle(rotation: 90);

            plt.XAxis.DateTimeFormat(true);
            plt.XAxis.SetSizeLimit(min: 50);
            plt.XAxis.Label(label);
            plt.SaveFig($"{plotFileName}.png");

        }
    }
}

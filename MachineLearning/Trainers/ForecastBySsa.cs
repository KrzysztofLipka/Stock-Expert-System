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
    public class SSASolveResult {
        public double Mae { get; set; }
        public double Rmse { get; set; }
        public double Acf { get; set; }
        public SsaForecastingTransformer Model { get; set; }
        //public int WindowSize { get; set; }
        //public int BestWindowSize { get; set; }
    }

    public class ForecastBySsa
    {

        public SsaForecastOutput Predict(string companyName, int horizon, DateTime maxDate = new DateTime())
        {
            ForecastBySsaParams parameters  = new ForecastBySsaParams() { Horizon = horizon };
            return Predict("", "", companyName, parameters, 0, false, maxDate);
        }

        public SsaForecastOutput PredictByWindowSize(string companyName, int horizon, DateTime maxDate = new DateTime())
        {
            ForecastBySsaParams parameters = new ForecastBySsaParams() { 
                Horizon = horizon,
                WindowSize = 100 };
            //var res = ssa.PredictByWindowSize("", "", "AAPL", parameters, 500, false, maxDate, true, "rys10");
            return PredictByWindowSize("", "", companyName, parameters, 0, false, maxDate);
        }

        private void SplitData(
            MLContext context, List<StockDataPointInput> data ,
            out IDataView trainSet, out IDataView testSet,
            int splitIndex) {

            List<StockDataPointInput> trainList;
            List<StockDataPointInput> testList;

            //todo use generic method
            SplitList(data, out trainList, out testList, splitIndex);
            trainSet = context.Data.LoadFromEnumerable<StockDataPointInput>(trainList);
            testSet = context.Data.LoadFromEnumerable<StockDataPointInput>(testList);
            //trainListSize = trainList.Count();
        }

        

        public SsaForecastOutput Predict(
            string dataPath, 
            string modelOuptutPath, 
            string companyName, 
            ForecastBySsaParams parameters, 
            int numberOfRowsToLoad, 
            bool saveModel = false, 
            DateTime maxDate = new DateTime(),
            bool plotResult = false,
            string finalPlotName = "plot"
            ) 
        {

            int horizon = parameters.Horizon;
            var context = new MLContext();
            int numberOfRows;
            DateTime lastUpdate;

            if (numberOfRowsToLoad == 0) {

                numberOfRowsToLoad = horizon< 365
                    ?  horizon * 15
                    :  horizon * 3;
            }

            var dataLoader = new DbDataLoader();
            IDataView dataFromDb = maxDate < DateTime.Today
                ?dataLoader.LoadDataFromDb(context, companyName, out numberOfRows,out lastUpdate, maxDate, numberOfRowsToLoad) 
                :dataLoader.LoadDataFromDb(context, companyName,out numberOfRows, out lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            double BestMAE = 9999;//nonIterativeModel.Mae;
            double BestRMSE = 9999;// nonIterativeModel.Rmse;
            double BestACF = 9999;//nonIterativeModel.Acf;
            int BestWindow = 0;//nonIterativeModel.WindowSize;
            int bestDataToSkipValue = 0;

            int splitIndex;
            int trainSize;
            int seriesLength;

            SSASolveResult finalModel;
            
                
                for (int dataToSkip = 0; dataToSkip < numberOfRowsToLoad-(horizon*2+3); dataToSkip += horizon)
                {
                    
                    List<StockDataPointInput> skippedDataForDbArray = dataFromDbToArray.Skip(dataToSkip).ToList();
                    splitIndex = (int)(skippedDataForDbArray.Count() * 0.8);
                    //splitIndex = skippedDataForDbArray.Count()- horizon*2;
                    trainSize = splitIndex;
                    int defaultWindowSize = trainSize/2-1;
                    int step = horizon == 365 ? 50 : 1;
                    seriesLength = 2* horizon +1;

                    for (int windowSize = defaultWindowSize - 1; windowSize > 2; windowSize -= step)
                    {
                        if (trainSize <= defaultWindowSize * 2 || seriesLength < defaultWindowSize)
                        {
                            continue;
                        }

                        SSASolveResult result = Solve(context, skippedDataForDbArray, skippedDataForDbArray.Count(), horizon, windowSize, trainSize, seriesLength);

                        if (result.Mae < BestMAE && result.Rmse < BestRMSE /*&&  Math.Abs(result.Acf) < 0.05*/ )
                        {
                            BestMAE = result.Mae;
                            BestRMSE = result.Rmse;
                            BestACF = result.Acf;
                            BestWindow = windowSize;
                            bestDataToSkipValue = dataToSkip;
                        };
                    }          
                }

            
            
            List<StockDataPointInput> bestSkippedDataForDbArray = dataFromDbToArray.Skip(bestDataToSkipValue).ToList();

            splitIndex = (int)(bestSkippedDataForDbArray.Count() * 0.8);
            //splitIndex = bestSkippedDataForDbArray.Count() - horizon*2;
            trainSize = splitIndex;
            seriesLength = 2 * horizon + 1; ;
            finalModel = Solve(context, bestSkippedDataForDbArray, bestSkippedDataForDbArray.Count(), horizon, BestWindow, trainSize, seriesLength,plotResult,finalPlotName);

            
            var forecastingEngine = finalModel.Model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(context);

            //tod verify if required
            //foreach (var forecast in testList)
            //{
            //    forecastingEngine.Predict(forecast);
            //}
            if (saveModel) {
                forecastingEngine.CheckPoint(context, modelOuptutPath + $"{lastUpdate.ToShortDateString()}.zip");
            }

            var res2 = forecastingEngine.Predict();


            return new SsaForecastOutput()
            {
                //Result = testList.Select(el => (double)el.ClosingPrice).ToArray(),
                Result = res2.Forecast.Select(el => (double)el).ToArray(),
                Mae = finalModel.Mae,
                Acf = finalModel.Acf,
                Rmse = finalModel.Rmse
            };
        }


        public SsaForecastOutput PredictByWindowSize(
             string dataPath,
            string modelOuptutPath,
            string companyName,
            ForecastBySsaParams parameters,
            int numberOfRowsToLoad,
            bool saveModel = false,
            DateTime maxDate = new DateTime(),
            bool plotResult = false,
            string finalPlotName = "plot") {

            int horizon = parameters.Horizon;
            int maxWindowSize = parameters.WindowSize;
            var context = new MLContext();
            int numberOfRows;
            DateTime lastUpdate;
            int seriesLength = 2 * horizon + 1;

            double BestMAE = 9999;//nonIterativeModel.Mae;
            double BestRMSE = 9999;// nonIterativeModel.Rmse;
            double BestACF = 9999;//nonIterativeModel.Acf;
            int BestWindow = 0;//nonIterativeModel.WindowSize;
     

            if (numberOfRowsToLoad == 0)
            {

                numberOfRowsToLoad = horizon < 365
                    ? horizon * 15
                    : horizon * 3;
            }

            var dataLoader = new DbDataLoader();

            IDataView dataFromDb = maxDate < DateTime.Today
                ? dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, maxDate, numberOfRowsToLoad)
                : dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            int splitIndex = (int)(dataFromDbToArray.Count() * 0.8);
            //splitIndex = skippedDataForDbArray.Count()- horizon*2;
            int trainSize = splitIndex;


            for (int windowSize = maxWindowSize - 1; windowSize > 2; windowSize -= 1)
            {
                if (trainSize <= windowSize * 2 || seriesLength <= windowSize)
                {
                    continue;
                }

                SSASolveResult result = Solve(context, dataFromDbToArray, dataFromDbToArray.Count(), horizon, windowSize, trainSize, seriesLength);

                if (result.Mae < BestMAE && result.Rmse < BestRMSE /*&&  Math.Abs(result.Acf) < 0.05*/ )
                {
                    BestMAE = result.Mae;
                    BestRMSE = result.Rmse;
                    BestACF = result.Acf;
                    BestWindow = windowSize;
                   
                };
            }

           
            trainSize = splitIndex;
            seriesLength = 2 * horizon + 1; ;
            SSASolveResult finalModel = Solve(context, dataFromDbToArray, dataFromDbToArray.Count(), horizon, BestWindow, trainSize, seriesLength, plotResult, finalPlotName);


            var forecastingEngine = finalModel.Model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(context);

            //tod verify if required
            //foreach (var forecast in testList)
            //{
            //    forecastingEngine.Predict(forecast);
            //}
            if (saveModel)
            {
                forecastingEngine.CheckPoint(context, modelOuptutPath + $"{lastUpdate.ToShortDateString()}.zip");
            }

            var res2 = forecastingEngine.Predict();


            return new SsaForecastOutput()
            {
                //Result = testList.Select(el => (double)el.ClosingPrice).ToArray(),
                Result = res2.Forecast.Select(el => (double)el).ToArray(),
                Mae = finalModel.Mae,
                Acf = finalModel.Acf,
                Rmse = finalModel.Rmse
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
               ? dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out  lastUpdate, maxDate, numberOfRowsToLoad)
               : dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out  lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            return Solve(context, dataFromDbToArray, numberOfRows, horizon, defaultWindowSize, trainSize, seriesLength, true, plotFileName);

            
             
        }

        private SSASolveResult Solve(
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

        /*public void UpdateModel(NbpData[] updateData, string modelOuptutPath) {
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
        }*/

        void Evaluate(IDataView testData, ITransformer model, MLContext mlContext, string label, out double MAE, out double RMSE, out double ACF, bool plotData = false, string plotFileNme = "plot")
        {
            IDataView predictions = model.Transform(testData);

            IEnumerable<StockDataPointInput> actualPoints = mlContext.Data.CreateEnumerable<StockDataPointInput>(testData, true);

            IEnumerable<float> actual = actualPoints.Select(observed => observed.ClosingPrice);
            IEnumerable<DateTime> actualDates = actualPoints.Select(observed => observed.Date);

            IEnumerable<float> forecast = mlContext.Data.CreateEnumerable<NbpForecastOutput>(predictions, true)
                .Select(prediction => prediction.Forecast[0]);
           
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

           MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
           RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
           ACF = MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(forecast, 2, x => x * x);

            if (plotData) {
                PlotData(actual.Count(), actual, forecast, label, MAE, RMSE, ACF, plotFileNme, actualDates);
            }
          

           Console.WriteLine("Evaluation Metrics");
           Console.WriteLine("---------------------");
           Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
           Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
           Console.WriteLine($"Auto Correlation Function: {ACF:F3}\n");

        }

 
        //todo
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

        private void PlotData(
            int horizon, IEnumerable<float> testList,
            IEnumerable<float> forecastsList, string label,
            double mae, double rmse,
            double acf, string plotFileName,
            IEnumerable<DateTime> dates = null
            ) {
            double[] y = dates!= null
                ? dates.Select(date=> date.ToOADate()).ToArray()
                : DataGen.Consecutive(horizon);
            double[] testArray = Array.ConvertAll(testList.ToArray(), x => (double)x);
            double[] forecastsArray = Array.ConvertAll(forecastsList.ToArray(), x => (double)x);

            var plt = new ScottPlot.Plot(1500, 500);
            //plt.AddSignal(testArray, horizon);

            //plt.AddSignal(forecastsArray, horizon);

            plt.AddScatter( y, testArray);

            plt.AddScatter(y,forecastsArray);



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

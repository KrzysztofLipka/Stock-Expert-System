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
        public int WindowSize { get; set; }
        public int BestWindowSize { get; set; }
    }

    public class ForecastBySsa
    {

        public SsaForecastOutput Predict(string companyName, int horizon, DateTime maxDate = new DateTime(), bool interateWindow = false)
        {
            ForecastBySsaParams parameters  = new ForecastBySsaParams() { Horizon = horizon };
            return Predict("", "", companyName, parameters, 0, false, maxDate, interateWindow);
        }

        public SsaForecastOutput Predict(string companyName, int horizon,int numberOfRowsToLoad, DateTime maxDate = new DateTime(), bool interateWindow = false)
        {
            ForecastBySsaParams parameters = new ForecastBySsaParams() { Horizon = horizon };
            return Predict("", "", companyName, parameters, 0, false, maxDate);
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

        private SSASolveResult Solve(MLContext context, List<StockDataPointInput> dataFromDbToArray, int numberOfRows,int horizon, bool iterateWindow, int defaultWindowSize, int trainSize) {
            var splitIndex = (int)(numberOfRows * 0.8);

            IDataView trainSet; //= context.Data.LoadFromEnumerable<StockDataPointInput>(trainList); 
            IDataView testSet; //= context.Data.LoadFromEnumerable<StockDataPointInput>(testList);

            //int trainListSize;

            SplitData(context, dataFromDbToArray, out trainSet, out testSet, splitIndex);
            //int trainSize = splitIndex; //365
            //int seriesLength = trainSize/3; //30
           
            //int windowSize = defaultWindowSize != 0 
            //    ? defaultWindowSize 
            //    : horizon+1;
            //windowSize = windowSize <= 2 ? 2 : windowSize;

            //if (seriesLength <= windowSize) {
            //    windowSize = seriesLength - 3;
            //}
            

            double Mae;
            double Rmse;
            double Acf;

            double BestMAE;
            double BestRMSE;
            double BestACF;
            int BestWindow = 0;

            SsaForecastingEstimator pipeline = context.Forecasting.ForecastBySsa(
                  "Forecast",
                 "ClosingPrice",
                 windowSize: defaultWindowSize, //windowSize,
                 seriesLength: 2*(horizon + 1),//windowSize*2, //seriesLength,
                 trainSize: trainSize,
                 horizon: horizon);

            SsaForecastingTransformer model = pipeline.Fit(trainSet);

            string plotLabel = $"windowsize: {defaultWindowSize}, " +
                    $"seriesLength: { 2 * (horizon + 1)}, trainSize: {trainSize}, " +
                    $"horizon: {horizon}, całkowita ilośc danych: {numberOfRows}";

            Evaluate(testSet, model, context, plotLabel, out Mae, out Rmse, out Acf);

            BestMAE = Mae;
            BestRMSE = Rmse;
            BestACF = Acf;

            for (int i = defaultWindowSize - 1; i > 2; i--)
            {
                

                int hor = i < 2 ? 2 : i;

                //if (seriesLength < i)
                //{
                //    hor = seriesLength - 1;
                //}

                pipeline = context.Forecasting.ForecastBySsa(
                    "Forecast",
                   "ClosingPrice",
                   windowSize: i,
                   seriesLength: 2 * (i + 1),//seriesLength,
                   trainSize: trainSize,
                   horizon: horizon);




                //var model = pipeline.Fit(dataFromDb);
                 model = pipeline.Fit(trainSet);

                

                string plotLabel2 = $"windowsize: {i}, " +
                    $"seriesLength: {2 * (i + 1)}, trainSize: {trainSize}, " +
                    $"horizon: {horizon}, całkowita ilośc danych: {numberOfRows}";

                Evaluate(testSet, model, context, plotLabel2, out Mae, out Rmse, out Acf);

                if (Mae < BestMAE && Rmse < BestRMSE /*&&  Math.Abs(Acf) < 0.05*/)
                {
                    BestMAE = Mae;
                    BestRMSE = Rmse;
                    BestACF = Acf;
                    BestWindow = hor;
                    //bestTrainListSize = dataToSkip;
                };


                if (!iterateWindow) {
                    break;
                }
            }

            return new SSASolveResult()
            {
                Acf = BestACF,
                Rmse = BestRMSE,
                Mae = BestMAE,
                WindowSize = defaultWindowSize,
                BestWindowSize = BestWindow,
                Model = model
            };

        }

        public SsaForecastOutput Predict(
            string dataPath, 
            string modelOuptutPath, 
            string companyName, 
            ForecastBySsaParams parameters, 
            int numberOfRowsToLoad, 
            bool saveModel = true, 
            DateTime maxDate = new DateTime(), 
            bool interateWindow = false) {

            int horizon = parameters.Horizon;
            var context = new MLContext();
            int numberOfRows;
            DateTime lastUpdate;

            if (numberOfRowsToLoad == 0) {

                numberOfRowsToLoad = horizon< 365
                    ?  horizon * 15
                    :  horizon * 3;
            }
            
            IDataView dataFromDb = maxDate < DateTime.Today
                ?DbDataLoader.LoadDataFromDb(context, companyName, out numberOfRows,out lastUpdate, maxDate, numberOfRowsToLoad) 
                :DbDataLoader.LoadDataFromDb(context, companyName,out numberOfRows, out lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            double BestMAE = 9999;//nonIterativeModel.Mae;
            double BestRMSE = 9999;// nonIterativeModel.Rmse;
            double BestACF = 9999;//nonIterativeModel.Acf;
            int BestWindow = 9999;//nonIterativeModel.WindowSize;
            int bestDataToSkipValue = 0;
            SSASolveResult finalModel;
            if (interateWindow) {
                
              
                for (int dataToSkip = 0; dataToSkip < numberOfRowsToLoad-(horizon*2+3); dataToSkip += horizon)
                {
                    List<StockDataPointInput> skippedDataForDbArray = dataFromDbToArray.Skip(dataToSkip).ToList();
                    var splitIndex = (int)(skippedDataForDbArray.Count() * 0.8);
                    int trainSize = splitIndex;
                    int defaultWindowSize = horizon + 1;

                    if (trainSize <= defaultWindowSize * 2) {
                        continue;
                    }


                    SSASolveResult result = Solve(context, skippedDataForDbArray, skippedDataForDbArray.Count(), horizon, true,defaultWindowSize,splitIndex);

                    if (result.Mae < BestMAE && result.Rmse <BestRMSE/* &&  Math.Abs(result.Acf) < 0.05*/ ) {
                        BestMAE = result.Mae;
                        BestRMSE = result.Rmse;
                        BestACF = result.Acf;
                        BestWindow = result.BestWindowSize;
                        bestDataToSkipValue = dataToSkip;
                    };

                    
                }

            }

            List<StockDataPointInput> bestSkippedDataForDbArray = dataFromDbToArray.Skip(bestDataToSkipValue).ToList();
            int finalSplitIndex = (int)(bestSkippedDataForDbArray.Count() * 0.8);
            finalModel = Solve(context, bestSkippedDataForDbArray, bestSkippedDataForDbArray.Count(), horizon, false, BestWindow, finalSplitIndex);

            
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

           //PlotData(actual.Count(), actual, forecast, label,MAE, RMSE,ACF);

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

        private void PlotData(int horizon, IEnumerable<float>testList, IEnumerable<float> forecastsList, string label, double mae, double rmse, double acf) {
            double[] y = DataGen.Consecutive(horizon);
            double[] testArray = Array.ConvertAll(testList.ToArray(), x => (double)x);
            double[] forecastsArray = Array.ConvertAll(forecastsList.ToArray(), x => (double)x);

            var plt = new ScottPlot.Plot(1500, 500);
            plt.AddSignal(testArray, horizon);

            plt.AddSignal(forecastsArray, horizon);
            plt.AddAnnotation($"MAE: {mae:F3} \nRMSE: {rmse:F3} \nACF: {acf:F3}", 10, -10);

            plt.XAxis.Label(label);
            plt.SaveFig("console22.png");

        }

    }

}

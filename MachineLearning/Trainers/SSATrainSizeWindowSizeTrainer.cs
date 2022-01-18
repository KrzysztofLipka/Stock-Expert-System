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

    public class SSATrainSizeWindowSizeTrainer : SSATrainerBase
    {

        public override SsaForecastOutput Forcast(string companyName, int horizon, DateTime maxDate = new DateTime())
        {
            ForecastBySsaParams parameters  = new ForecastBySsaParams() { Horizon = horizon };
            return Forcast("", "", companyName, parameters, 0, false, maxDate);
        }

       
       
        public override SsaForecastOutput Forcast(
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

                numberOfRowsToLoad = parameters.Horizon switch
                {
                    5 => 100,
                    30 => 400,
                    365 => 500,
                    _ => 500
                };
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
                    seriesLength = trainSize;

                    for (int windowSize = defaultWindowSize - 1; windowSize > 2; windowSize -= step)
                    {
                        if (trainSize <= defaultWindowSize * 2 || seriesLength <= defaultWindowSize)
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
            seriesLength = trainSize;
            finalModel = Solve(context, bestSkippedDataForDbArray, bestSkippedDataForDbArray.Count(), horizon, BestWindow, trainSize, seriesLength,plotResult,finalPlotName);

            
            var forecastingEngine = finalModel.Model.CreateTimeSeriesEngine<StockDataPointInput, ForecastOutput>(context);

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

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
    public class SSAWindowsSizeTrainer : SSATrainerBase
    {
        public override SsaForecastOutput Forcast(string companyName, int horizon, DateTime maxDate = new DateTime())
        {
            ForecastBySsaParams parameters = new ForecastBySsaParams()
            {
                Horizon = horizon,
                WindowSize = 100
            };
            //var res = ssa.PredictByWindowSize("", "", "AAPL", parameters, 500, false, maxDate, true, "rys10");
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
           string finalPlotName = "plot")
        {

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


            var forecastingEngine = finalModel.Model.CreateTimeSeriesEngine<StockDataPointInput, ForecastOutput>(context);

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


    }
}

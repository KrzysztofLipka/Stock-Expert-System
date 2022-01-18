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
    public class SSAConstantParametersTrainer : SSATrainerBase
    {
        public override SsaForecastOutput Forcast(string companyName, int horizon, DateTime maxDate = default)
        {

            ForecastBySsaParams inputParameters = GetInputParameters(horizon);
            return Forcast("","",companyName, inputParameters,0,false,maxDate, false,"plot");
        }

        public override SsaForecastOutput Forcast(string dataPath, string modelOuptutPath, string companyName, ForecastBySsaParams parameters, int numberOfRowsToLoad, bool saveModel = false, DateTime maxDate = default, bool plotResult = false, string finalPlotName = "plot")
        {
            MLContext context = new MLContext();

            var dataLoader = new DbDataLoader();

            if (numberOfRowsToLoad == 0)
            {
                numberOfRowsToLoad = parameters.Horizon switch
                {
                    5 => 200,
                    30 => 450,
                    365 => 1000,
                    _ => 500
                };
             
            }


            int numberOfRows;
            DateTime lastUpdate;
            IDataView dataFromDb = maxDate < DateTime.Today
                ? dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, maxDate, numberOfRowsToLoad)
                : dataLoader.LoadDataFromDb(context, companyName, out numberOfRows, out lastUpdate, numberOfRowsToLoad);
            List<StockDataPointInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            SSASolveResult model = Solve(
                context,
                dataFromDbToArray,
                numberOfRows, parameters.Horizon,
                parameters.WindowSize, 
                parameters.TrainSize, 
                parameters.SeriesLength, 
                true, "constParams");


            var forecastingEngine = model.Model.CreateTimeSeriesEngine<StockDataPointInput, ForecastOutput>(context);

            var result = forecastingEngine.Predict();

            return new SsaForecastOutput() {
            Rmse = model.Rmse,
            Mae = model.Mae,
            Result = result.Forecast.Select(el => (double)el).ToArray(),
            };




        }

        private ForecastBySsaParams GetInputParameters(int horizon) {
            int dataCount = horizon switch
            {
                5 => 50,
                30 => 450,
                365 => 1000,
                _ => 500
            };
            if (horizon == 5)
            {
                return new ForecastBySsaParams()
                {
                    WindowSize = 10,
                    SeriesLength = dataCount,
                    TrainSize = dataCount,
                    Horizon = 5
                };
            }
            else if (horizon == 30)
            {
                return new ForecastBySsaParams()
                {
                    WindowSize = 32,
                    TrainSize = dataCount,
                    SeriesLength = dataCount,
                    Horizon = 30
                };
            }
            else {
                return new ForecastBySsaParams()
                {
                    WindowSize = 40,
                    TrainSize = dataCount,
                    SeriesLength = dataCount,
                    Horizon = 365
                };

            }
        }
    }
}

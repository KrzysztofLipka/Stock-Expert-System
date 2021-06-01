using System;
using Microsoft.ML;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;

namespace MachineLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new MLContext();

            var data = context.Data.LoadFromTextFile<NbpData>("./test.csv", 
                hasHeader: false, 
                separatorChar: ',');

            var pipeline = context.Forecasting.ForecastBySsa(
                "Forecast",
               nameof(NbpData.ExchangeRate),
               windowSize: 5,
               seriesLength: 10,
               trainSize: 100,
               horizon: 4);

            var model = pipeline.Fit(data);

            var forecastingEngine = model.CreateTimeSeriesEngine<NbpData, NbpForecastOutput>(context);

            var forecasts = forecastingEngine.Predict();

            foreach (var forecast in forecasts.Forecast)
            {
                Console.WriteLine(forecast);
            }

            forecastingEngine.CheckPoint(context, "../../../../MLModels/forecast_model.zip");

            Console.ReadLine();



        }
    }
}

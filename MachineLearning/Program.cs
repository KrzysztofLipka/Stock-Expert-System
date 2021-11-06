using System;
using Microsoft.ML;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;
using MachineLearning.Trainers;
using System.Collections.Generic;
using Externals;

namespace MachineLearning
{
    class Program
    {
        static void Main(string[] args)
        {
           
            //ApiHelper.InitClient();
            //var i = AlphaVantageService.GetRatesForStock("AAPL");

            //StoqqService service = new StoqqService();

            //service.getGata();

            ForecastBySsa forecast = new ForecastBySsa();

            forecast.Predict("../../../../MLModels/GPW_GPB.csv", 
                            "../../../../MLModels/forecast_model.zip", "AAPL");


            //double[] realvalues =  new double[] { 5.0087, 5.0072, 4.9761, 5.0092, 5.0013 };
            //var mlContext = new MLContext();

            //var testSetTransform = trainedModel.Transform(dataSplit.TestSet);

            //var metrics = mlContext.MulticlassClassification.Evaluate();


            //forecast.UpdateModel(new NbpData[] {
            //    new NbpData(){
            //        Date = "1111111",
            //        ExchangeRate = 123
            //    }
            //}, "../../../../MLModels/forecast_model.zip"); 

            Console.ReadKey();

        }
    }
}

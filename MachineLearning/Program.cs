using System;
using Microsoft.ML;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;
using MachineLearning.Trainers;
using System.Collections.Generic;
using Externals;
using ScottPlot;

namespace MachineLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            const string companyName = "nke";
            const int horizon = 30;


            ForecastBySsa forecast = new ForecastBySsa();

            BinaryClassifier binary = new BinaryClassifier();

            binary.Predict("1", "2", "nke", "^dji", 2000);

            //binary.Predict("1", "2", "AAPL", "^ndq", 2000);
            //Console.WriteLine(i);
            ForecastBySsaParams parameters = new ForecastBySsaParams()
                {
                    Horizon = 30,
                };

               
             var res = forecast.Predict("../../../../MLModels/.csv",
                                $"../../../../MLModels/SSA_{companyName}_{horizon}_", companyName,
                                parameters,
                                450
                                );
           
            Console.WriteLine("---------------");
            Console.ReadKey();

        }

        /*private void PredictForMultuipleTrainSizes() {
            ForecastBySsa forecast = new ForecastBySsa();


            double mae;
            double rmse;
            double acf;

            double minMae = 99999;
            double minRmse = 999999;

            int minMaeSize = 0;
            int minRsmeSize = 0;

            List<double> rmseRecords = new List<double>();
            List<double> maeRecords = new List<double>();
            int numberOfRecords = 0;



            for (int i = 1200; i < 2000; i++)
            {

                //Console.WriteLine(i);
                ForecastBySsaParams parameters = new ForecastBySsaParams()
                {
                    Horizon = 30,
                    TrainSize = i,
                    WindowSize = i,
                    //SeriesLength = rowNumber

                };

                forecast.Predict("../../../../MLModels/.csv",
                                "../../../../MLModels/forecast_model.zip", "rds-a",
                                parameters,
                                450,
                                out mae,
                                out rmse,
                                out acf
                                );
                rmseRecords.Add(rmse);
                maeRecords.Add(mae);
                numberOfRecords++;



                if (mae < minMae)
                {
                    minMae = mae;
                    minMaeSize = i;
                }

                if (rmse < minRmse)
                {
                    minRmse = rmse;
                    minRsmeSize = i;
                }

            }

            double[] y = DataGen.Consecutive(numberOfRecords);
            for (int i = 0; i < y.Length; i++)
            {
                y[i] += 350;
            }


            var plt = new ScottPlot.Plot(1500, 500);
            plt.AddScatter(y, rmseRecords.ToArray());

            plt.AddScatter(y, maeRecords.ToArray());

            plt.XAxis.Label("Horizon = 30, Window = trainSize/3 AAPL trainSize");
            plt.SaveFig("console5.png");

            Console.WriteLine("---------------");
            Console.WriteLine(minRmse);
            Console.WriteLine(minMae);

            Console.WriteLine(minMaeSize);
            Console.WriteLine(minRsmeSize);

            Console.ReadKey();

        }*/
    }
}

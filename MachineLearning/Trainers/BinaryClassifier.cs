using System;
using System.Collections.Generic;
using System.Text;

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
using MachineLearning.Util;
using Microsoft.ML.Trainers;

namespace MachineLearning.Trainers
{
    internal class Prediction
    {
        // Original label.
        public bool Label { get; set; }
        // Predicted label from the trainer.
        public bool PredictedLabel { get; set; }
    }

    public class BinaryClassifier
    {
        string connectionString = "";



        private IDataView LoadDataFromDb(MLContext context, string companyName, string indexName, out int numberOfRows, int numberOfRowsToLoad, /*out DateTime lastUpdated,*/ DateTime? requestedUpdateDate = null)
        {
            DatabaseLoader loader = context.Data.CreateDatabaseLoader<StockDataPointInput>();
            //string sqlCommand = "select ClosingPrice from dbo.HistoricalPrices where CompanyId = 1 order by PriceId";
            string sqlCommand = $"EXEC dbo.GetClosingPrices @CompanyName = '{companyName}'";
            string sqlCommand2 = $"EXEC dbo.GetClosingPricesWithMaxDate @CompanyName = '{companyName}', @MaxDate = '03-01-2021'";
            string sqlCommand3 = $"EXEC dbo.[GetLastClosingPrices] @CompanyName = '{companyName}', @NumberOfLastRows = {numberOfRowsToLoad}";
            string sqlCommand4 = $"EXEC dbo.GetClosingPricesWithMinMaxDate @CompanyName = '{companyName}', @MaxDate = '03-05-2016', @MinDate = '03-05-2021'";


            //EXEC dbo.GetClosingPricesWithMinMaxDate @CompanyName = 'AAPL', @MinDate = '03-05-2021', @MaxDate = '03-05-2020'

            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand4);

            IDataView dataFromDb = loader.Load(dbSource);

            //numberOfRows = dataFromDb.Preview(20000).RowView.Length;
            var data1 = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var data2 = data1.Skip(Math.Max(0, data1.Count() - numberOfRowsToLoad));

            //---index data-------------------

          
            DatabaseSource dbSource2 = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlCommand4);

            IDataView dataFromDb2 = loader.Load(dbSource);

                //numberOfRows = dataFromDb.Preview(20000).RowView.Length;
            var data12 = context.Data.CreateEnumerable<StockDataPointInput>(dataFromDb, reuseRowObject: false).ToList();

            var data22 = data1.Skip(Math.Max(0, data1.Count() - numberOfRowsToLoad));


            

            //-------------------------------

            var data3 = BinaryConverter.Convert(data2, data22);


            //lastUpdated = data3.Last().Date;

            numberOfRows = data2.Count();



            var data4 = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(data3);

            var normalize = context.Transforms.NormalizeMeanVariance("Features",
                fixZero: false);

            var normalizeTransform = normalize.Fit(data4);

            var transformedData = normalizeTransform.Transform(data4);


            //var data5 = 

            return transformedData;
        }

        public void Predict(string dataPath, string modelOuptutPath, string companyName, int numberOfRowsToLoad) {
            var mlContext = new MLContext();

            
            var context = new MLContext();
            int numberOfRows;
            DateTime lastUpdate;
            IDataView dataFromDb = LoadDataFromDb(context, companyName,"^dji", out numberOfRows, numberOfRowsToLoad);

            //var pip = mlContext.Transforms.NormalizeMinMax("Features", "Features");

            List<StockDataPointBinaryInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointBinaryInput>(dataFromDb, reuseRowObject: false).ToList();

            List<StockDataPointBinaryInput> trainList;
            List<StockDataPointBinaryInput> testList;

            SplitList(dataFromDbToArray, out trainList, out testList, 600);

            var trainSet = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(trainList);

            var testSet = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(testList);

            //---------------------------------

            var options = new LdSvmTrainer.Options
            {
                TreeDepth = 5,
                NumberOfIterations = 30000,
                Sigma = 0.1f,
                //FeatureColumnName = "PriceDate",
                //LabelColumnName = "IsRising"
            };

            var pipeline = mlContext.BinaryClassification.Trainers
                .LdSvm(options);

            var model = pipeline.Fit(trainSet);

            var transformedTestData = model.Transform(testSet);
            var predictions = mlContext.Data
              .CreateEnumerable<Prediction>(transformedTestData,
              reuseRowObject: false).ToList();

            foreach (var p in predictions.Take(100))
                Console.WriteLine($"Label: {p.Label}, "
                    + $"Prediction: {p.PredictedLabel}");

            var metrics = mlContext.BinaryClassification
                .EvaluateNonCalibrated(transformedTestData);

            PrintMetrics(metrics);
            var t = model.Model.InputType;
          

        }

        private void SplitList(
           List<StockDataPointBinaryInput> inputList,
           out List<StockDataPointBinaryInput> listOne,
           out List<StockDataPointBinaryInput> listTwo,
           int splitIndex
           )
        {
            var res1 = new List<StockDataPointBinaryInput>();
            var res2 = new List<StockDataPointBinaryInput>();

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

        private static void PrintMetrics(BinaryClassificationMetrics metrics)
        {
            Console.WriteLine($"Accuracy: {metrics.Accuracy:F2}");
            Console.WriteLine($"AUC: {metrics.AreaUnderRocCurve:F2}");
            Console.WriteLine($"F1 Score: {metrics.F1Score:F2}");
            Console.WriteLine($"Negative Precision: " +
                $"{metrics.NegativePrecision:F2}");

            Console.WriteLine($"Negative Recall: {metrics.NegativeRecall:F2}");
            Console.WriteLine($"Positive Precision: " +
                $"{metrics.PositivePrecision:F2}");

            Console.WriteLine($"Positive Recall: {metrics.PositiveRecall:F2}\n");
            Console.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());
        }

        private List<float> CalculateMomentum(int numberOfDays, List<double> priceArray) {
            int daysAhead = 270;
            List<float> momentumList = new List<float>();
            List<int> movingMomentumList = new List<int>();
            for (int i = 1; i < numberOfDays+ 1; i++)
            {
                movingMomentumList.Add(priceArray[i]> priceArray[i -1] ? 1 : -1);
            }

            momentumList.Add(momentumList.Average());

            for (int j = numberOfDays + 1; j < priceArray.Count- daysAhead; j++)
            {
                movingMomentumList.RemoveAt(0);
                movingMomentumList.Add(priceArray[j] > priceArray[j - 1] ? 1 : -1);
                momentumList.Add(momentumList.Average());
            }

            return momentumList;

        }

    }
}

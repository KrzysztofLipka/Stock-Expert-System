using System;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using System.Linq;
using MachineLearning.Util;
using Microsoft.ML.Trainers;
using MachineLearning.DataLoading;

namespace MachineLearning.Trainers
{
    internal class Prediction
    {
        public bool Label { get; set; }
        public bool PredictedLabel { get; set; }
    }

    public class BinaryClassifier
    {
        private IDataView PrepateInputData(
            MLContext context, string companyName, 
            string indexName, out int numberOfRows, 
            int numberOfRowsToLoad,
            int numberOfDays, int daysAhead,
            DateTime? requestedUpdateDate = null)
        {
            int numberCompanyDataRows;
            int numberIndexDataRows;

            DateTime minDate = new DateTime(2012, 5, 3);
            DateTime maxDate = new DateTime(2021, 5, 3);
            var dataLoader = new DbDataLoader();
            IDataView companyData = dataLoader.LoadDataFromDb(context, companyName, out numberCompanyDataRows, minDate, maxDate);
            IDataView indexData = dataLoader.LoadDataFromDb(context, indexName, out numberIndexDataRows, minDate, maxDate);

            var companyDataAsList = context.Data.CreateEnumerable<StockDataPointInput>(companyData, reuseRowObject: false);
            companyDataAsList = companyDataAsList.Skip(Math.Max(0, companyDataAsList.Count() - numberOfRowsToLoad));

            var indexDataAsList = context.Data.CreateEnumerable<StockDataPointInput>(indexData, reuseRowObject: false);
            indexDataAsList = indexDataAsList.Skip(Math.Max(0, indexDataAsList.Count() - numberOfRowsToLoad));

            var connectedData = BinaryConverter.Convert(companyDataAsList, indexDataAsList, numberOfDays, daysAhead);
            numberOfRows = companyDataAsList.Count();
            IDataView connectedDataview = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(connectedData);
            var normalize = context.Transforms.NormalizeMeanVariance("Features",
                fixZero: false);

            var normalizeTransform = normalize.Fit(connectedDataview);
            var transformedData = normalizeTransform.Transform(connectedDataview);

            return transformedData;
        }

        public void Predict(string dataPath, string modelOuptutPath, 
            string companyName,string indexName, 
            int numberOfRowsToLoad, int numberOfDays, int daysAhead) {

            Console.WriteLine($"*----numberOfDays: {numberOfDays}-----daysAhead: {daysAhead}------------*");

            //var mlContext = new MLContext();

            var context = new MLContext(seed:1);
            int numberOfRows;
            DateTime lastUpdate;
            IDataView dataFromDb = PrepateInputData(context, companyName, indexName, out numberOfRows, numberOfRowsToLoad, numberOfDays, daysAhead);


            List<StockDataPointBinaryInput> dataFromDbToArray = context.Data.CreateEnumerable<StockDataPointBinaryInput>(dataFromDb, reuseRowObject: false).ToList();

            List<StockDataPointBinaryInput> trainList;
            List<StockDataPointBinaryInput> testList;

            SplitList(dataFromDbToArray, out trainList, out testList, 600);

            var trainSet = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(trainList);

            var testSet = context.Data.LoadFromEnumerable<StockDataPointBinaryInput>(testList);

            var pos = testList.Where(el => el.IsRising).ToList().Count();
            var neg = testList.Where(el => !el.IsRising).ToList().Count();
            

            Console.WriteLine($"pos: {pos} neg: {neg}");


            //---------------------------------

            var options = new LdSvmTrainer.Options
            {
                TreeDepth = 5,
                NumberOfIterations = 30000,
                Sigma = 0.1f,
                //FeatureColumnName = "PriceDate",
                //LabelColumnName = "IsRising"
            };

            var pipeline = context.BinaryClassification.Trainers
                .LdSvm(options);

            var model = pipeline.Fit(trainSet);

            var transformedTestData = model.Transform(testSet);
            var predictions = context.Data
              .CreateEnumerable<Prediction>(transformedTestData,
              reuseRowObject: false).ToList();

            //foreach (var p in predictions.Take(100))
            //    Console.WriteLine($"Label: {p.Label}, "
            //        + $"Prediction: {p.PredictedLabel}");

            var metrics = context.BinaryClassification
                .EvaluateNonCalibrated(transformedTestData);

            PrintMetrics(metrics);
            var t = model.Model.InputType;
          

        }

        //todo use generic
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

    }
}

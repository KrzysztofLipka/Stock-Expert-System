using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MathNet.Numerics;
using MachineLearning.Util;

namespace MachineLearning.Trainers
{
    public class ArimaData {
        public ArimaData(float closingPrice, string date)
        {
            this.ClosingPrice = closingPrice;
            this.Date = date;
        }

        public ArimaData(string date, float closingPrice)
        {
            this.ClosingPrice = closingPrice;
            this.Date = date;
        }

        public ArimaData()
        {
 
        }
        public float ClosingPrice;
        public string Date;
    }

    public class ArimaTrainTestSplittedData
    {
        public List<List<float>> xTrain;
        public List<List<float>> xTest;
        public List<float> yTrain;
        public List<float> yTest;

        public ArimaTrainTestSplittedData(
        List<List<float>> xTrain,
        List<List<float>> xTest,
        List<float> yTrain,
        List<float> yTest)
        {
            this.xTrain = xTrain;
            this.xTest = xTest;
            this.yTrain = yTrain;
            this.yTest = yTest;
        }
    }

    public class ArimaTrainData {
        public List<List<float>> xTrain;
        public List<float> yTrain;

        public ArimaTrainData(
            List<List<float>> xTrain, 
            List<float> yTrain)
        {
            this.xTrain = xTrain;
            this.yTrain = yTrain;
        }
    }
    // todo remove
    public class RevertingResult {
        public double[] orginalValues;
        public double[] predictedValues;
    }

    public class ArimaAutoRegressionResult {
        public List<double> PredictedTrainValues { get; set; }
        public List<double> PredictedTestValues { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public List<float> RegressionResults { get; set; }
        public double[][] XTrain { get; set; }
        public double[][] XTest { get; set; }

        public double[][] Coefficients { get; set; }


    }

    public class ArimaMovingAverageResult
    {
        public List<double> PredictedTrainValues { get; set; }
        public List<double> PredictedTestValues { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public List<float> RegressionResults { get; set; }
        public double[][] XTrain { get; set; }
        public double[][] XTest { get; set; }
        public double[][] Coefficients { get; set; }
    }


    public static class Arima
    {
        public static List<List<float>> CreateLaggedVectors(int p, List<ArimaData> df) {
            List<List<float>> laggedVectors = new List<List<float>>();
            for (int i = 1; i < p+1; i++)
            {
                var shifted = ShiftFloat(i, df);
                laggedVectors.Add(shifted);
            }

            return laggedVectors;

        }

        public static List<List<float>> CreateLaggedVectors(int p, IEnumerable<float> df)
        {
            List<List<float>> laggedVectors = new List<List<float>>();
            for (int i = 1; i < p + 1; i++)
            {
                var shifted = ShiftFloat(i, df);
                laggedVectors.Add(shifted);
            }

            return laggedVectors;

        }

        public static List<ArimaData> PrepareInputData(List<ArimaData> input) {
            // todo move date
            var inputAsFloat = input.Select(row => Math.Log(row.ClosingPrice));
            var stageOne = MatrixHelpers.CalculateDiffrence(inputAsFloat.ToList(), 1);
            var res = MatrixHelpers.CalculateDiffrence(stageOne,12);
            //todo remove hack
            var res2 =  res.Select(row => new ArimaData((float)row, "Data")).ToList();
            res2[12].ClosingPrice = 0;

            return res2;
        }

        public static RevertingResult RevertToOrginalData(double[] predictions,double[] orginalDiffrencedValues, IEnumerable<float> orginalValues)
        {
            //todo simplify
            var inputLogOrg = ShiftDouble(1, orginalValues.Select(row => Math.Log(row)));
            var sumOfPredictedOrginalValuesAndInputLog = SumArraysWithDiffrentSize(orginalDiffrencedValues, inputLogOrg.ToArray());
            var inputLog2Org = ShiftDouble(12, MatrixHelpers.CalculateDiffrence(orginalValues.Select(row => Math.Log(row)).ToList(), 1));
            var sumOfPredictedOrginalValuesAndInputLog2 = SumArraysWithDiffrentSize(sumOfPredictedOrginalValuesAndInputLog.ToArray(), inputLog2Org.ToArray());


            var inputLog = ShiftDouble(1,orginalValues.Select(row => Math.Log(row)));
            var sumOfPredictedValuesAndInputLog = SumArraysWithDiffrentSize(predictions, inputLog.ToArray());
            var inputLog2 = ShiftDouble(12,MatrixHelpers.CalculateDiffrence(orginalValues.Select(row => Math.Log(row)).ToList(),1));
            var sumOfPredictedValuesAndInputLog2 = SumArraysWithDiffrentSize(sumOfPredictedValuesAndInputLog.ToArray(), inputLog2.ToArray());

            sumOfPredictedOrginalValuesAndInputLog2 = sumOfPredictedOrginalValuesAndInputLog2.Skip(orginalValues.Count() - predictions.Count()).Select(el => Math.Exp(el)).ToArray();
            sumOfPredictedValuesAndInputLog2 = sumOfPredictedValuesAndInputLog2.Select(el => Math.Exp(el)).ToArray();
           
            return new RevertingResult()
            {
                orginalValues = sumOfPredictedOrginalValuesAndInputLog2,
                predictedValues = sumOfPredictedValuesAndInputLog2
            };
        }
        

        public static void Solve(int p, int q, List<ArimaData> df) {

            List<ArimaData> dfLog = PrepareInputData(df);
            int arSpitIndex = (int)((double)dfLog.Count * 0.8);
         
            var splitedDataSet = SplitToTrainTestData(p, dfLog, arSpitIndex);
                

            var autoRegressionResult = AutoRegression(p, dfLog, splitedDataSet);

                     //todo
            //List<float> residualsFloat = residuals.Skip(p).Select(x => (float)x).ToList();

            List<float> skippedInitialValues = dfLog.Skip(p+13).Select(x => (float)x.ClosingPrice).ToList();

            double[] concatinatedXTrainTest = autoRegressionResult.PredictedTrainValues.Concat(autoRegressionResult.PredictedTestValues).ToArray();

            double[] residuals = CreateResiduals(concatinatedXTrainTest, skippedInitialValues);

            int maSpitIndex = (int)((double)residuals.Count() * 0.8);

            var movingAverageSplittedDataSet = SplitToTrainTestData(q, residuals.Select(res => (float)res).ToList(), maSpitIndex, false);
 
            var movingAverageResult = MovingAverage(q, residuals, movingAverageSplittedDataSet);

            var concatinatedMovingAverageXTrainTest = movingAverageResult.PredictedTrainValues.Concat(movingAverageResult.PredictedTestValues).ToArray();
            var sumOfResults = SumArraysWithDiffrentSize(concatinatedXTrainTest, concatinatedMovingAverageXTrainTest);
            var finalResult = RevertToOrginalData(sumOfResults, dfLog.Select(el=> (double)el.ClosingPrice).ToArray(), df.Select(el => el.ClosingPrice));

            //---------forecasting---------------

            List<double> dataForForecast = new List<double>();
            List<double> originalDataForForecast = new List<double>();
            List<double> finalForecasts = new List<double>();
            double[] coef = autoRegressionResult.Coefficients.Select(el => el[0]).ToArray();
            originalDataForForecast.AddRange(finalResult.orginalValues.Skip(finalResult.orginalValues.Length - p-1).Reverse().ToList());

            for (int i = 0; i < originalDataForForecast.Count() -1; i++)
            {
                dataForForecast.Add((float)originalDataForForecast[i] - (float)originalDataForForecast[i + 1]);
            }

            int numberOfForecasts = 16;

            for (int i = 0; i < numberOfForecasts; i++)
            {
                double resu = originalDataForForecast[0];
                double sum = dataForForecast.Zip(coef, (price, coefEl) => price * coefEl).Sum();
                resu += sum;
                //resu + dataForForecast.Zip()
                finalForecasts.Add(resu);
                originalDataForForecast.Insert(0, resu);
                originalDataForForecast = originalDataForForecast.Take(originalDataForForecast.Count() - 1).ToList();

                dataForForecast.Insert(0, originalDataForForecast[0] - originalDataForForecast[1]);
                dataForForecast = dataForForecast.Take(dataForForecast.Count() - 1).ToList();


            }

            //--------------Ma  forecast------------

            var revertedErrors = RevertToOrginalData(sumOfResults, concatinatedMovingAverageXTrainTest, df.Select(el => el.ClosingPrice));

            List<double> dataForForecast2 = new List<double>();
            List<double> originalDataForForecast2 = new List<double>();
            List<double> finalForecasts2 = new List<double>();
            double[] coef2 = movingAverageResult.Coefficients.Select(el => el[0]).ToArray();
            originalDataForForecast2.AddRange(revertedErrors.predictedValues.Skip(revertedErrors.predictedValues.Count() - q - 1).Reverse().ToList());

            for (int i = 0; i < originalDataForForecast2.Count() - 1; i++)
            {
                dataForForecast2.Add((float)originalDataForForecast2[i] - (float)originalDataForForecast2[i + 1]);
            }

            //int numberOfForecasts = 5;

            for (int i = 0; i < numberOfForecasts; i++)
            {
                double resu = 1;
                double sum = dataForForecast2.Zip(coef2, (price, coefEl) => price * coefEl).Sum();
                resu += sum;
                finalForecasts2.Add(resu);
                originalDataForForecast2.Insert(0, resu);
                originalDataForForecast2 = originalDataForForecast2.Take(originalDataForForecast2.Count() - 1).ToList();

                dataForForecast2.Insert(0, originalDataForForecast2[0] - originalDataForForecast2[1]);
                dataForForecast2 = dataForForecast2.Take(dataForForecast2.Count() - 1).ToList();


            }

            var forecastplusErros = finalForecasts.Zip(finalForecasts2, (first, second) => first + second);
            Console.WriteLine("forcastsum");
            foreach (var item in forecastplusErros)
            {
                Console.WriteLine(item);
            }
        }


        public static double[] SumArraysWithDiffrentSize(double[] firstArray, double[] secondArray) {
           List<double> result = new List<double>();
            int coundDiference = firstArray.Length - secondArray.Length;
            //result.InsertRange(0, new double[firstArray.Length - secondArray.Length]);
            for (int i = 0; i < firstArray.Length; i++)
            {
                if (i < coundDiference)
                {
                    result.Add(0);
                }
                else {
                    result.Add(firstArray[i]+ secondArray[i-coundDiference]);
                }
            }

            return result.ToArray();
        }

        public static float[] SumArraysWithDiffrentSize(float[] firstArray, float[] secondArray)
        {
            List<float> result = new List<float>();
            int coundDiference = firstArray.Length - secondArray.Length;
            //result.InsertRange(0, new double[firstArray.Length - secondArray.Length]);
            for (int i = 0; i < firstArray.Length; i++)
            {
                if (i < coundDiference)
                {
                    result.Add(0);
                }
                else
                {
                    result.Add(firstArray[i] + secondArray[i - coundDiference]);
                }
            }

            return result.ToArray();

        }

        public static double[] CreateResiduals(double[] autoRegressionResult, List<ArimaData> df) {
            return df.Zip(autoRegressionResult, (x, y) => (double)x.ClosingPrice - y).ToArray();
        }

        public static double[] CreateResiduals(double[] autoRegressionResult, List<float> df)
        {
            return df.Zip(autoRegressionResult, (x, y) => (double)x - y).ToArray();
        }

        public static ArimaMovingAverageResult MovingAverage(int q, double[] residuals, ArimaTrainTestSplittedData data) {
       
            double[][] xTrainAsArray = data.xTrain.Select(column => column.Select(el => (double)el).ToArray()).ToArray();

            double[][] xTrainArr = Transpose(data.xTrain);

            double[] yTrainAsAray = data.yTrain.Select(el => (double)el).ToArray();

            double[] r = Fit.MultiDim(xTrainArr, yTrainAsAray);

            var transposedXTrain = Transpose(data.xTrain);

            var transposedRegressionResult = Transpose(r);

            var dotP = MatrixHelpers.DotProduct(transposedXTrain, transposedRegressionResult);

            var transposedXTest = Transpose(data.xTest);

            var testDotp = MatrixHelpers.DotProduct(transposedXTest, transposedRegressionResult);

            var testDotpAsList = testDotp.Select(item => item[0]);

            var metrics = testDotpAsList.Zip(data.yTest, (actualValue, forecastValue) => actualValue - forecastValue);

            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            Console.WriteLine($"MAE: {MAE }");
            Console.WriteLine($"RMSE: {RMSE }");

            return new ArimaMovingAverageResult() {
                RMSE = RMSE,
                MAE = MAE,
                XTrain = transposedXTrain,
                XTest = transposedXTest,
                PredictedTrainValues = dotP.Select(prediction => prediction[0]).ToList(),
                Coefficients = transposedRegressionResult,
                PredictedTestValues = testDotpAsList.ToList()
            };


        } 

        public static ArimaAutoRegressionResult AutoRegression(int p, List<ArimaData> df, ArimaTrainTestSplittedData data) {
            double[][] xTrainAsArray = data.xTrain.Select(column => column.Select(el => (double)el).ToArray()).ToArray();

            double[][] xTrainArr = Transpose(data.xTrain);

            double[] yTrainAsAray = data.yTrain.Select(el => (double)el).ToArray();

            var transposedXTrain = Transpose(data.xTrain);

            double[] r = Fit.MultiDim(xTrainArr, yTrainAsAray);

            var transposedRegressionResult = Transpose(r);
            var dotP = MatrixHelpers.DotProduct(transposedXTrain, transposedRegressionResult);
            var transposedXTest = Transpose(data.xTest);
            var testDotp = MatrixHelpers.DotProduct(transposedXTest, transposedRegressionResult);
            var testDotpAsList = testDotp.Select(item => item[0]);

            var metrics = testDotpAsList.Zip(data.yTest, (actualValue, forecastValue) => actualValue - forecastValue);
            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            return new ArimaAutoRegressionResult()
            {
                RMSE = RMSE,
                MAE = MAE,
                XTrain = transposedXTrain,
                XTest = transposedXTest,
                PredictedTrainValues = dotP.Select(prediction => prediction[0]).ToList(),

                PredictedTestValues = testDotpAsList.ToList(),
                Coefficients = transposedRegressionResult
            };
        }

        public static double[] AddCoef(double[] Input, double Coef) {
            return Input.Select(el => el + Coef).ToArray();
        }

        public static List<List<float>> DropZeros(int p,List<List<float>> laggedVectors, bool skipAdditionalRows = true) {
            List<List<float>> result = new List<List<float>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;

            return laggedVectors.Select(column => column.Skip(skipAdditionalRows? p+ 13 :p).ToList()).ToList();
        }

        public static List<List<double>> DropZeros(int q, List<List<double>> laggedVectors)
        {
            List<List<float>> result = new List<List<float>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;
           
            return laggedVectors.Select(column => column.Skip(q).ToList()).ToList();
        }

        public static double[][] Transpose(IEnumerable<IEnumerable<float>> xData) {
            double[][] result = 
                xData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => (double)i.item)
                .Select(g => g.ToArray())
                .ToArray();
            return result;
        }

        public static double[][] Transpose(IEnumerable<IEnumerable<double>> xData)
        {
            double[][] result =
                xData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => i.item)
                .Select(g => g.ToArray())
                .ToArray();
            return result;
        }

        public static double[][] Transpose(IEnumerable<double> xData) {
            return xData.Select(el=> new double[] { el }).ToArray();
        }

        public static List<List<float>>[] SplitLaggedVectors(List<List<float>> laggedVectors,int splitIndex) {
            List<List<float>> xTrain = new List<List<float>>();
            List<List<float>> xTest = new List<List<float>>();
            
            foreach (var column in laggedVectors)
            {
                List<List<float>> splittedColumn = SplitColumn(column, splitIndex);
                xTrain.Add(splittedColumn[0]);
                xTest.Add(splittedColumn[1]);
            }
            return new List<List<float>>[2] { xTrain, xTest };
        }

        public static List<List<float>> SplitColumn(List<float> df, int splitIndex) {
            return df.Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index < splitIndex).Select(x => x.Select(v => v.Value).ToList()).ToList();
            
        }

        public static ArimaTrainTestSplittedData PrepareTrainData(int p, IEnumerable<float> df, bool skipAdditionalRows = true) {
            List<List<float>> laggedVectors = new List<List<float>>();
            List<List<float>> dataForPrediction = new List<List<float>>();
            
            laggedVectors = CreateLaggedVectors(p, df);
           
            var xData = DropZeros(p, laggedVectors, skipAdditionalRows);
            int l = laggedVectors[0].Count();
         
            int index;
            var dfAsList = df.ToList();
            for (int i = 0; i < p; i++)
            {
                index = l - i;
                List<float> record = new List<float>();
               
                for (int j = index-1; j >= index-p; j--)
                {
                    Console.WriteLine(j);
                    record.Add(dfAsList[j]);
                }
                dataForPrediction.Add(record);
            }
            var yData = df.Select(row => row).Skip(skipAdditionalRows ? p + 13 : p).ToList(); ;
            return new ArimaTrainTestSplittedData(xData, dataForPrediction , yData, null);
        }

        public static ArimaTrainTestSplittedData PrepareTrainData(int p, List<ArimaData> df, bool skipAdditionalRows = true)
        {
            List<List<float>> laggedVectors = new List<List<float>>();
            List<List<float>> dataForPrediction = new List<List<float>>();
            laggedVectors = CreateLaggedVectors(p, df);
            var xData = DropZeros(p, laggedVectors, skipAdditionalRows);

            int l = laggedVectors[0].Count();
            int index;
            var dfAsList = df.ToList();
            for (int i = 0; i < p; i++)
            {
                index = l - i;
                List<float> record = new List<float>();

                for (int j = index - 1; j >= index - p; j--)
                {
                    record.Add(dfAsList[j].ClosingPrice);
                }
                dataForPrediction.Add(record);
            }
            var yData = df.Select(row => row.ClosingPrice).Skip(skipAdditionalRows ? p + 13 : p).ToList(); ;
            return new ArimaTrainTestSplittedData(xData, dataForPrediction, yData, null);
        }

        public static ArimaTrainTestSplittedData SplitToTrainTestData(int p, List<ArimaData> df, int splitIndex, bool skipAdditionalRows = true) {
                List<List<float>> laggedVectors = new List<List<float>>();
                List<List<float>> xTrain = new List<List<float>>();
                List<List<float>> xTest = new List<List<float>>();
                List<float> yTrain = new List<float>();
                List<float> yTest = new List<float>();

                laggedVectors = CreateLaggedVectors(p, df);
                var xData = SplitLaggedVectors(laggedVectors, splitIndex);
                //todo
                var yData = SplitColumn(df.Select(row => row.ClosingPrice).ToList(), splitIndex);

                xTrain = DropZeros(p,xData[0],skipAdditionalRows);
                xTest = xData[1];

                yTrain = yData[0].Skip(skipAdditionalRows? p + 13 :p).ToList();
                yTest = yData[1];
            
                return new ArimaTrainTestSplittedData(xTrain, xTest, yTrain, yTest);
            
            }

        public static ArimaTrainTestSplittedData SplitToTrainTestData(int p, IEnumerable<float> df, int splitIndex, bool skipAdditionalRows = true)
        {
            List<List<float>> laggedVectors = new List<List<float>>();
            List<List<float>> xTrain = new List<List<float>>();
            List<List<float>> xTest = new List<List<float>>();
            List<float> yTrain = new List<float>();
            List<float> yTest = new List<float>();

            laggedVectors = CreateLaggedVectors(p, df);
            var xData = SplitLaggedVectors(laggedVectors, splitIndex);
            var yData = SplitColumn(df.Select(row => row).ToList(), splitIndex);

            xTrain = DropZeros(p, xData[0],skipAdditionalRows);
            xTest = xData[1];

            yTrain = yData[0].Skip(p).ToList();
            yTest = yData[1];

            return new ArimaTrainTestSplittedData(xTrain, xTest, yTrain, yTest);

        }



        public static List<ArimaData> Shift(int periods ,List<ArimaData> df) {
            float[] b = new float[periods];
            
            List<float> closingPrices = df
                .Select(row => row.ClosingPrice)
                .Skip(periods).ToList();
            closingPrices.InsertRange(0, new float[periods]);
            //df.ForEach(row => row.ClosingPrice = closingPri)
            return df.Zip(closingPrices, (oldPrice, price) => new ArimaData()
            {
                ClosingPrice = price,
                Date = oldPrice.Date
            }).ToList();
        }

        public static List<float> ShiftFloat(int periods, List<ArimaData> df)
        {
            float[] b = new float[periods];

            List<float> closingPrices = df
                .Select(row => row.ClosingPrice).ToList();
                //.Skip(periods).ToList();
            closingPrices.InsertRange(0, new float[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }

        public static List<float> ShiftFloat(int periods, IEnumerable<float> df)
        {
            float[] b = new float[periods];

            List<float> closingPrices = df
                .Select(row => row).ToList();
            //.Skip(periods).ToList();
            closingPrices.InsertRange(0, new float[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }

        public static List<double> ShiftDouble(int periods, IEnumerable<double> df)
        {
            double[] b = new double[periods];

            List<double> closingPrices = df
                .Select(row => row).ToList();
            //.Skip(periods).ToList();
            closingPrices.InsertRange(0, new double[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }

        public static List<float> Shift(int periods, IEnumerable<float> df)
        {
            float[] b = new float[periods];
            List<float> closingPrices = df
                .Select(row => row).ToList();
            //.Skip(periods).ToList();
            closingPrices.InsertRange(0, new float[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }
    }
}

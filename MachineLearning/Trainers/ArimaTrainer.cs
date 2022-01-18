using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MachineLearning.Util;
using MachineLearning.DataModels;
using MachineLearning.DataLoading;

namespace MachineLearning.Trainers
{
    public class ArimaTrainTestSplittedData
    {
        public List<List<double>> xTrain;
        public List<List<double>> xTest;
        public List<double> yTrain;
        public List<double> yTest;

        public ArimaTrainTestSplittedData(
        List<List<double>> xTrain,
        List<List<double>> xTest,
        List<double> yTrain,
        List<double> yTest)
        {
            this.xTrain = xTrain;
            this.xTest = xTest;
            this.yTrain = yTrain;
            this.yTest = yTest;
        }
    }

    public class ArimaTrainData {
        public List<List<double>> xTrain;
        public List<double> yTrain;

        public ArimaTrainData(
            List<List<double>> xTrain, 
            List<double> yTrain)
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

    public class CalculateCoefficientsResult {
        public List<double> PredictedTrainValues { get; set; }
        public List<double> PredictedTestValues { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public List<double> RegressionResults { get; set; }
        public double[][] XTrain { get; set; }
        public double[][] XTest { get; set; }

        public double[][] Coefficients { get; set; }

    }

    /*public class ArimaMovingAverageResult
    {
        public List<double> PredictedTrainValues { get; set; }
        public List<double> PredictedTestValues { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public List<double> RegressionResults { get; set; }
        public double[][] XTrain { get; set; }
        public double[][] XTest { get; set; }
        public double[][] Coefficients { get; set; }
    }*/


    public static class ArimaTrainer
    {
        public static List<List<double>> CreateLaggedVectors(int p, List<ArimaData> df) {
            List<List<double>> laggedVectors = new List<List<double>>();
            for (int i = 1; i < p+1; i++)
            {
                var shifted = Shift(i, df);
                laggedVectors.Add(shifted);
            }

            return laggedVectors;

        }

        public static List<List<double>> CreateLaggedVectors(int p, IEnumerable<double> df)
        {
            List<List<double>> laggedVectors = new List<List<double>>();
            for (int i = 1; i < p + 1; i++)
            {
                var shifted = Shift(i, df);
                laggedVectors.Add(shifted);
            }

            return laggedVectors;

        }

        public static List<ArimaData> PrepareInputData(List<ArimaData> input) {
            // todo move date
            var inputAsFloat = input.Select(row => Math.Log(row.ClosingPrice));
            var stageOne = MatrixHelpers.CalculateDiffrence(inputAsFloat.ToList(), 1);
            var res = MatrixHelpers.CalculateDiffrence(stageOne,12);
            //todo remove hack and parse data
            var res2 =  res.Select(row => new ArimaData(row, new DateTime())).ToList();
            res2[12].ClosingPrice = 0;

            return res2;
        }

        public static RevertingResult RevertToOrginalData(double[] predictions,double[] orginalDiffrencedValues, IEnumerable<double> orginalValues)
        {
            //todo simplify
            var inputLogOrg = Shift(1, orginalValues.Select(row => Math.Log(row)));
            var sumOfPredictedOrginalValuesAndInputLog = SumArraysWithDiffrentSize(orginalDiffrencedValues, inputLogOrg.ToArray());
            var inputLog2Org = Shift(12, MatrixHelpers.CalculateDiffrence(orginalValues.Select(row => Math.Log(row)).ToList(), 1));
            var sumOfPredictedOrginalValuesAndInputLog2 = SumArraysWithDiffrentSize(sumOfPredictedOrginalValuesAndInputLog.ToArray(), inputLog2Org.ToArray());


            var inputLog = Shift(1,orginalValues.Select(row => Math.Log(row)));
            var sumOfPredictedValuesAndInputLog = SumArraysWithDiffrentSize(predictions, inputLog.ToArray());
            var inputLog2 = Shift(12,MatrixHelpers.CalculateDiffrence(orginalValues.Select(row => Math.Log(row)).ToList(),1));
            var sumOfPredictedValuesAndInputLog2 = SumArraysWithDiffrentSize(sumOfPredictedValuesAndInputLog.ToArray(), inputLog2.ToArray());

            sumOfPredictedOrginalValuesAndInputLog2 = sumOfPredictedOrginalValuesAndInputLog2.Skip(orginalValues.Count() - predictions.Count()).Select(el => Math.Exp(el)).ToArray();
            sumOfPredictedValuesAndInputLog2 = sumOfPredictedValuesAndInputLog2.Select(el => Math.Exp(el)).ToArray();
           
            return new RevertingResult()
            {
                orginalValues = sumOfPredictedOrginalValuesAndInputLog2,
                predictedValues = sumOfPredictedValuesAndInputLog2
            };
        }

        public static IEnumerable<double> Solve(int p, int q, string companyName, int horizon, DateTime maxDate = new DateTime(), bool interateParameters = false) {
            //for endpoint
            int numberOfRows;
            int numberOfRowsToLoad = horizon == 365 
                ? horizon * 3  
                : horizon * 10;
            var dataLoader = new DbDataLoader();
            IEnumerable<StockDataPointInput> data = maxDate < DateTime.Today  
                ? dataLoader.LoadDataFromDb(companyName, out numberOfRows,maxDate, numberOfRowsToLoad)
                : dataLoader.LoadDataFromDb(companyName, out numberOfRows, numberOfRowsToLoad);



            return Solve(p, q, data, horizon, interateParameters);
        }

        public static IEnumerable<double> Solve(int p, int q, string companyName,DateTime maxDate, int horizon)
        {
            int numberOfRows;
            int numberOfRowsToLoad = horizon * 3;
            var dataLoader = new DbDataLoader();
            IEnumerable<StockDataPointInput> data = dataLoader.LoadDataFromDb(
                companyName, out numberOfRows,
                maxDate, numberOfRowsToLoad);

            return Solve(p, q, data, horizon);
        }

        public static IEnumerable<double> Solve(int p, int q, IEnumerable<StockDataPointInput>df ,int horizon = 1, bool interateParameters = false) {
            var input = ConvertFromStockDataPointInput(df);
            return Solve(p, q, input, horizon, interateParameters);
        }

        private static List<ArimaData> ConvertFromStockDataPointInput(IEnumerable<StockDataPointInput> df) {
            return df.Select(el => new ArimaData(el.ClosingPrice, el.Date)).ToList();
        }
        

        public static IEnumerable<double> Solve(int p, int q, List<ArimaData> df, int horizon = 1, bool interateParameters = false) {

            List<ArimaData> dfLog = PrepareInputData(df);
            int arSpitIndex = (int)(dfLog.Count * 0.8);
            int selectedP = p;
            int selectedQ = q;

            ArimaTrainTestSplittedData splitedDataSet;
            CalculateCoefficientsResult autoRegressionResult;

            if (interateParameters) {
                splitedDataSet = SplitToTrainTestData(1, dfLog, arSpitIndex);
                autoRegressionResult = AutoRegression(splitedDataSet);
                double RMSE = autoRegressionResult.RMSE;
                for (int i = 2; i < p; i++)
                {
                    var splitedDataSetOption = SplitToTrainTestData(i, dfLog, arSpitIndex);
                    var autoRegressionResultOption = AutoRegression(splitedDataSetOption);
                    if (autoRegressionResultOption.RMSE < RMSE)
                    {
                        splitedDataSet = splitedDataSetOption;
                        autoRegressionResult = autoRegressionResultOption;
                        RMSE = autoRegressionResultOption.RMSE;
                        selectedP = i;
                    }
                   
                }
            }
            else
            {
                splitedDataSet = SplitToTrainTestData(selectedP, dfLog, arSpitIndex);
                autoRegressionResult = AutoRegression(splitedDataSet);
            };

            //todo
            List<double> skippedInitialValues = dfLog.Skip(selectedP+13).Select(x => x.ClosingPrice).ToList();

            double[] concatinatedXTrainTest = autoRegressionResult.PredictedTrainValues.Concat(autoRegressionResult.PredictedTestValues).ToArray();

            double[] residuals = CreateResiduals(concatinatedXTrainTest, skippedInitialValues);

            int maSpitIndex = (int)(residuals.Count() * 0.8);

            //todo
            //ArimaTrainTestSplittedData movingAverageSplittedDataSet = SplitToTrainTestData(q, residuals.Select(res =>res).ToList(), maSpitIndex, false);
            //ArimaMovingAverageResult movingAverageResult = MovingAverage(q, residuals, movingAverageSplittedDataSet);
            ArimaTrainTestSplittedData movingAverageSplittedDataSet;
            CalculateCoefficientsResult movingAverageResult;
            if (interateParameters)
            {
                movingAverageSplittedDataSet = SplitToTrainTestData(1, residuals.Select(res => res).ToList(), maSpitIndex, false);
                movingAverageResult = AutoRegression(movingAverageSplittedDataSet);
                double MAE = movingAverageResult.MAE;

                for (int i = 2; i < q; i++)
                {
                    var splitedDataSetOption = SplitToTrainTestData(i, residuals.Select(res => res).ToList(), maSpitIndex, false);
                    var movingAverageResultOption = AutoRegression(movingAverageSplittedDataSet);
                    if (movingAverageResultOption.MAE < MAE)
                    {
                        splitedDataSet = splitedDataSetOption;
                        movingAverageResult = movingAverageResultOption;
                        MAE = movingAverageResult.MAE;
                        selectedQ = i;
                    }

                }

            }
            else {
                movingAverageSplittedDataSet = SplitToTrainTestData(selectedQ, residuals.Select(res => res).ToList(), maSpitIndex, false);
                movingAverageResult = AutoRegression(movingAverageSplittedDataSet);
            }
            

            var concatinatedMovingAverageXTrainTest = movingAverageResult.PredictedTrainValues.Concat(movingAverageResult.PredictedTestValues).ToArray();
            var sumOfResults = SumArraysWithDiffrentSize(concatinatedXTrainTest, concatinatedMovingAverageXTrainTest);
            var finalResult = RevertToOrginalData(sumOfResults, dfLog.Select(el=> el.ClosingPrice).ToArray(), df.Select(el => el.ClosingPrice));

            //---------forecasting---------------

            List<double> dataForForecast = new List<double>();
            List<double> originalDataForForecast = new List<double>();
            List<double> finalForecasts = new List<double>();
            double[] coef = autoRegressionResult.Coefficients.Select(el => el[0]).ToArray();
            originalDataForForecast.AddRange(finalResult.orginalValues.Skip(finalResult.orginalValues.Length - selectedP-1).Reverse().ToList());

            for (int i = 0; i < originalDataForForecast.Count() -1; i++)
            {
                dataForForecast.Add(originalDataForForecast[i] - originalDataForForecast[i + 1]);
            }

            //int numberOfForecasts = 16;

            for (int i = 0; i < horizon; i++)
            {
                double resu = originalDataForForecast[0]+ autoRegressionResult.RMSE;
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
            originalDataForForecast2.AddRange(revertedErrors.predictedValues.Skip(revertedErrors.predictedValues.Count() - selectedQ - 1).Reverse().ToList());

            for (int i = 0; i < originalDataForForecast2.Count() - 1; i++)
            {
                dataForForecast2.Add(originalDataForForecast2[i] - originalDataForForecast2[i + 1]);
            }

            for (int i = 0; i < horizon; i++)
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

            return forecastplusErros;
            //return finalForecasts;
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

        public static double[] CreateResiduals(double[] autoRegressionResult, List<ArimaData> df) {
            return df.Zip(autoRegressionResult, (x, y) => x.ClosingPrice - y).ToArray();
        }

        public static double[] CreateResiduals(double[] autoRegressionResult, List<double> df)
        {
            return df.Zip(autoRegressionResult, (x, y) => x - y).ToArray();
        }

        /*public static ArimaMovingAverageResult MovingAverage(int q, double[] residuals, ArimaTrainTestSplittedData data) {
       
            double[][] xTrainAsArray = data.xTrain.Select(column => column.ToArray()).ToArray();
            double[][] xTrainArr = Transpose(data.xTrain);
            double[] yTrainAsAray = data.yTrain.ToArray();
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
        } */

        public static CalculateCoefficientsResult AutoRegression(ArimaTrainTestSplittedData data) {
            double[][] xTrainAsArray = data.xTrain.Select(column => column.ToArray()).ToArray();
            double[][] xTrainArr = Transpose(data.xTrain);
            double[] yTrainAsAray = data.yTrain.ToArray();
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

            return new CalculateCoefficientsResult()
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

        public static List<List<double>> DropZeros(int p,List<List<double>> laggedVectors, bool skipAdditionalRows = true) {
            List<List<double>> result = new List<List<double>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;

            return laggedVectors.Select(column => column.Skip(skipAdditionalRows? p+ 13 :p).ToList()).ToList();
        }

        public static List<List<double>> DropZeros(int q, List<List<double>> laggedVectors)
        {
            List<List<double>> result = new List<List<double>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;
           
            return laggedVectors.Select(column => column.Skip(q).ToList()).ToList();
        }

        public static double[][] Transpose(IEnumerable<IEnumerable<double>> xData) {
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

        public static List<List<double>>[] SplitLaggedVectors(List<List<double>> laggedVectors,int splitIndex) {
            List<List<double>> xTrain = new List<List<double>>();
            List<List<double>> xTest = new List<List<double>>();
            
            foreach (var column in laggedVectors)
            {
                List<List<double>> splittedColumn = SplitColumn(column, splitIndex);
                xTrain.Add(splittedColumn[0]);
                xTest.Add(splittedColumn[1]);
            }
            return new List<List<double>>[2] { xTrain, xTest };
        }

        public static List<List<double>> SplitColumn(List<double> df, int splitIndex) {
            return df.Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index < splitIndex).Select(x => x.Select(v => v.Value).ToList()).ToList();
            
        }

        public static ArimaTrainTestSplittedData PrepareTrainData(int p, IEnumerable<double> df, bool skipAdditionalRows = true) {
            List<List<double>> laggedVectors = new List<List<double>>();
            List<List<double>> dataForPrediction = new List<List<double>>();
            
            laggedVectors = CreateLaggedVectors(p, df);
           
            var xData = DropZeros(p, laggedVectors, skipAdditionalRows);
            int l = laggedVectors[0].Count();
         
            int index;
            var dfAsList = df.ToList();
            for (int i = 0; i < p; i++)
            {
                index = l - i;
                List<double> record = new List<double>();
               
                for (int j = index-1; j >= index-p; j--)
                {
                    Console.WriteLine(j);
                    record.Add(dfAsList[j]);
                }
                dataForPrediction.Add(record);
            }
            //todo
            var yData = df.Select(row => row).Skip(skipAdditionalRows ? p + 13 : p).ToList(); ;
            return new ArimaTrainTestSplittedData(xData, dataForPrediction , yData, null);
        }

        public static ArimaTrainTestSplittedData PrepareTrainData(int p, List<ArimaData> df, bool skipAdditionalRows = true)
        {
            List<List<double>> laggedVectors = new List<List<double>>();
            List<List<double>> dataForPrediction = new List<List<double>>();
            laggedVectors = CreateLaggedVectors(p, df);
            var xData = DropZeros(p, laggedVectors, skipAdditionalRows);

            int l = laggedVectors[0].Count();
            int index;
            var dfAsList = df.ToList();
            for (int i = 0; i < p; i++)
            {
                index = l - i;
                List<double> record = new List<double>();

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
                List<List<double>> laggedVectors = new List<List<double>>();
                List<List<double>> xTrain = new List<List<double>>();
                List<List<double>> xTest = new List<List<double>>();
                List<double> yTrain = new List<double>();
                List<double> yTest = new List<double>();

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

        public static ArimaTrainTestSplittedData SplitToTrainTestData(int p, IEnumerable<double> df, int splitIndex, bool skipAdditionalRows = true)
        {
            List<List<double>> laggedVectors = new List<List<double>>();
            List<List<double>> xTrain = new List<List<double>>();
            List<List<double>> xTest = new List<List<double>>();
            List<double> yTrain = new List<double>();
            List<double> yTest = new List<double>();

            laggedVectors = CreateLaggedVectors(p, df);
            var xData = SplitLaggedVectors(laggedVectors, splitIndex);
            var yData = SplitColumn(df.Select(row => row).ToList(), splitIndex);

            xTrain = DropZeros(p, xData[0],skipAdditionalRows);
            xTest = xData[1];

            yTrain = yData[0].Skip(p).ToList();
            yTest = yData[1];

            return new ArimaTrainTestSplittedData(xTrain, xTest, yTrain, yTest);

        }

    

        public static List<double> Shift(int periods, List<ArimaData> df)
        {
            double[] b = new double[periods];

            List<double> closingPrices = df
                .Select(row => row.ClosingPrice).ToList();
                //.Skip(periods).ToList();
            closingPrices.InsertRange(0, new double[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }

        public static List<double> Shift(int periods, IEnumerable<double> df)
        {
            double[] b = new double[periods];
            //todo
            List<double> closingPrices = df
                .Select(row => row).ToList();
            closingPrices.InsertRange(0, new double[periods]);
            var res = closingPrices.SkipLast(periods);
            return res.ToList();
        }

    }
}
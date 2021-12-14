using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MathNet.Numerics;

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

    public class ArimaAutoRegressionResult {
        
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

        public static void Solve(int p, int q, List<ArimaData> df) {
            var splitedDataSet = Arima.SplitToTrainTestData(p, df, 13);
            var autoRegressionResult = AutoRegression(p, df, splitedDataSet);
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!");
            foreach (var item in autoRegressionResult)
            {
                Console.WriteLine(item);
            }
            double[] residuals =  CreateResiduals(autoRegressionResult, df);
            List<float> residualsFloat = residuals.Select(x => (float)x).ToList();

            var movingAverageSplittedDataSet = Arima.SplitToTrainTestData(q, residualsFloat, 13);
            var movingAverageResult = MovingAverage(q, residuals, movingAverageSplittedDataSet);
            //foreach (var item in movingAverageResult)
            //{
            //    Console.WriteLine(item);
            //}

            var res = autoRegressionResult.Zip(movingAverageResult, (firstRes, secondRes) => firstRes + secondRes);

            //Console.WriteLine("2!!!!!!!!!!!!!!!!!!!");
            //foreach (var item in  res)
            //{
            //    Console.WriteLine(item);
            //}





        }

        public static double[] CreateResiduals(double[] autoRegressionResult, List<ArimaData> df) {
            return df.Zip(autoRegressionResult, (x, y) => (double)x.ClosingPrice - y).ToArray();
        }

        public static double[] MovingAverage(int q, double[] residuals, ArimaTrainTestSplittedData data) {
            //List<List<float>> laggedValues = new List<List<float>>();
            //List<float> residualsFloat = residuals.Select(x => (float)x).ToList(); 
            //for (int i = 1; i < q+1; i++)
            //{
            //    var shifted = Shift(q, residualsFloat);
            //    laggedValues.Add(shifted);
            //}

            //DropZeros(q, laggedValues);

            double[][] xTrainAsArray = data.xTrain.Select(column => column.Select(el => (double)el).ToArray()).ToArray();

            double[][] xTrainArr = Transpose(data.xTrain);

            double[] yTrainAsAray = data.yTrain.Select(el => Math.Round((double)el, 3)).ToArray();

            double[] r = Fit.MultiDim(xTrainArr, yTrainAsAray);

            var transposedXTrain = Transpose(data.xTrain);

            var transposedRegressionResult = Transpose(r);

            var dotP = DotProduct(transposedXTrain, transposedRegressionResult);

            return dotP.Select(el => el[0]).ToArray();


        } 



        public static double[] AutoRegression(int p, List<ArimaData> df, ArimaTrainTestSplittedData data) {
            //List<List<float>> laggedVectors = CreateLaggedVectors(p, df);
           
            //var data = SplitToTrainTestData(p, df, 6);
            double[][] xTrainAsArray = data.xTrain.Select(column => column.Select(el => (double)el).ToArray()).ToArray();

            double[][] xTrainArr = Transpose(data.xTrain);

            double[] yTrainAsAray = data.yTrain.Select(el => Math.Round((double)el,3)).ToArray();
          
            Console.WriteLine("xTrainArr");

            foreach (var item in xTrainArr)
            {
                Console.WriteLine($"{item[0]} {item[1]}");
            }

            Console.WriteLine("yTrainAsAray");

            foreach (var item in yTrainAsAray)
            {
                Console.WriteLine(item);
            }

            double[] r = Fit.MultiDim(xTrainArr, yTrainAsAray);

            var transposedXTrain = Transpose(data.xTrain);

            var transposedRegressionResult = Transpose(r);

            var dotP = DotProduct(transposedXTrain, transposedRegressionResult);

            return dotP.Select(el => el[0]).ToArray();

        }

        public static double[][] DotProduct(double[][] FirstInputMatrix, double[][] SecondInputMatrix) {
            return FirstInputMatrix.Select( // goes through <lhs> row by row
            (row, rowIndex) =>
            SecondInputMatrix[0].Select( // goes through first row of <rhs> cell by cell
                (_, columnIndex) =>
                SecondInputMatrix.Select(__ => __[columnIndex]) // selects column from <rhs>
                    .Zip(row, (rowCell, columnCell) => rowCell * columnCell).Sum() // does scalar product
                ).ToArray()
            ).ToArray();
        }

        public static double[] AddCoef(double[] Input, double Coef) {
            return Input.Select(el => el + Coef).ToArray();
        }





        public static List<List<float>> DropZeros(int p,List<List<float>> laggedVectors) {
            List<List<float>> result = new List<List<float>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;
            //int pointsToSkip = 1;

            return laggedVectors.Select(column => column.Skip(p).ToList()).ToList();
        }

        public static List<List<double>> DropZeros(int q, List<List<double>> laggedVectors)
        {
            List<List<float>> result = new List<List<float>>();
            int numberOfColumns = laggedVectors.Count;
            int numberOfRows = laggedVectors[0].Count;
            //int pointsToSkip = 1;

            return laggedVectors.Select(column => column.Skip(q).ToList()).ToList();
        }

        public static double[][] Transpose(IEnumerable<IEnumerable<float>> xData) {
            double[][] result = 
                xData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => Math.Round((double)i.item,3))
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
            

            //for (int columnIndex = 0; columnIndex < df.Count; columnIndex++)
            //{
            //    if(laggedVectors[columnIndex])
            //}
            foreach (var column in laggedVectors)
            {
                //List<List<float>> splittedColumn = column.Select((x, i) => new { Index = i, Value = x })
                //     .GroupBy(x => x.Index < splitIndex).Select(x => x.Select(v => v.Value).ToList()).ToList();
                Console.WriteLine("!----------!");
                Console.WriteLine(column.Count);
                Console.WriteLine(splitIndex);
                List<List<float>> splittedColumn = SplitColumn(column, splitIndex);
                Console.WriteLine($"{splittedColumn[0]},{splittedColumn[1]}");

                xTrain.Add(splittedColumn[0]);
                xTest.Add(splittedColumn[1]);

            }
            return new List<List<float>>[2] { xTrain, xTest };
        }

        //public static List<float>[] splitInputData(List<float> df, int splitIndex) {
        //    List<float> yTrain = new List<float>();
        //    List<float> yTest = new List<float>();
            
        //}

        public static List<List<float>> SplitColumn(List<float> df, int splitIndex) {
            return df.Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index < splitIndex).Select(x => x.Select(v => v.Value).ToList()).ToList();
            
        }

        

    public static ArimaTrainTestSplittedData SplitToTrainTestData(int p, List<ArimaData> df, int splitIndex) {
            List<List<float>> laggedVectors = new List<List<float>>();
            List<List<float>> xTrain = new List<List<float>>();
            List<List<float>> xTest = new List<List<float>>();
            List<float> yTrain = new List<float>();
            List<float> yTest = new List<float>();

            laggedVectors = CreateLaggedVectors(p, df);
            var xData = SplitLaggedVectors(laggedVectors, splitIndex);
            var yData = SplitColumn(df.Select(row => row.ClosingPrice).ToList(), splitIndex);

            xTrain = DropZeros(p,xData[0]);
            xTest = xData[1];

            yTrain = yData[0].Skip(p).ToList();
            yTest = yData[1];
            
            return new ArimaTrainTestSplittedData(xTrain, xTest, yTrain, yTest);
            
        }

        public static ArimaTrainTestSplittedData SplitToTrainTestData(int p, IEnumerable<float> df, int splitIndex)
        {
            List<List<float>> laggedVectors = new List<List<float>>();
            List<List<float>> xTrain = new List<List<float>>();
            List<List<float>> xTest = new List<List<float>>();
            List<float> yTrain = new List<float>();
            List<float> yTest = new List<float>();

            laggedVectors = CreateLaggedVectors(p, df);
            var xData = SplitLaggedVectors(laggedVectors, splitIndex);
            var yData = SplitColumn(df.Select(row => row).ToList(), splitIndex);

            xTrain = DropZeros(p, xData[0]);
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

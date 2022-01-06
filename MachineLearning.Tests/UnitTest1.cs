using NUnit.Framework;
using MachineLearning.Trainers;
using MachineLearning.Util;
using System.Collections.Generic;
using System;
using System.Linq;
using MachineLearning.DataModels;

namespace MachineLearning.Tests
{
    [TestFixture]
    public class ArimaTests
    {
        List<ArimaData> testData;
        List<ArimaData> laggedVectorsTestData;
        List<float> splitTestData;

        [SetUp]
        public void Setup()
        {
            testData = new List<ArimaData>();
            testData.Add(new ArimaData(1.234f,new DateTime()));
            testData.Add(new ArimaData(1.236f, new DateTime()));
            testData.Add(new ArimaData(1.244f, new DateTime()));
            testData.Add(new ArimaData(1.222f, new DateTime()));
            testData.Add(new ArimaData(1.88f, new DateTime()));
            testData.Add(new ArimaData(1.244f, new DateTime()));
            testData.Add(new ArimaData(1.24f, new DateTime()));
            testData.Add(new ArimaData(1.28f, new DateTime()));
            testData.Add(new ArimaData(1.004f,new DateTime()));
            testData.Add(new ArimaData(1.24f, new DateTime()));
            testData.Add(new ArimaData(1.00f, new DateTime()));
            testData.Add(new ArimaData(1.23f, new DateTime()));
            testData.Add(new ArimaData(1.20f, new DateTime()));
            testData.Add(new ArimaData(1.24f, new DateTime()));
            testData.Add(new ArimaData(1.234f,new DateTime()));
            testData.Add(new ArimaData(1.284f,new DateTime()));


            laggedVectorsTestData = new List<ArimaData>();
            laggedVectorsTestData.Add(new ArimaData(1.111f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.222f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.333f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.444f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.555f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.666f, new DateTime()));
            laggedVectorsTestData.Add(new ArimaData(1.777f, new DateTime()));


            splitTestData = new List<float>();
            splitTestData.Add(1.111f);
            splitTestData.Add(2.222f);
            splitTestData.Add(3.333f);
            splitTestData.Add(4.444f);
            splitTestData.Add(5.555f);
            splitTestData.Add(6.666f);
            splitTestData.Add(7.777f);

        }

        [Test]
        public void Test1()
        {
            var expectedResult = new List<ArimaData> {
            new ArimaData(0, new DateTime()),
            new ArimaData(1.234f, new DateTime()),
            new ArimaData(1.236f, new DateTime()),
            new ArimaData(1.244f, new DateTime()),
            new ArimaData(1.222f, new DateTime()),
            new ArimaData(1.88f,  new DateTime()),
            new ArimaData(1.244f, new DateTime()),
            new ArimaData(1.24f,  new DateTime()),
            new ArimaData(1.28f,  new DateTime()),
            new ArimaData(1.004f, new DateTime()),
            new ArimaData(1.24f,  new DateTime()),
            new ArimaData(1.00f,  new DateTime()),
            new ArimaData(1.23f,  new DateTime()),
            new ArimaData(1.20f,  new DateTime()),
            new ArimaData(1.24f,  new DateTime()),
            new ArimaData(1.234f, new DateTime()),
            new ArimaData(1.284f, new DateTime())

        };

          


            var result = Arima.Shift(1, testData);
            List<double> resultAsFloatList = result.Select(item => item.ClosingPrice).ToList();
            List<double> expectedResultAsFloatList = expectedResult.Select(item => item.ClosingPrice).ToList();
            foreach (var item in result)
            {
                Console.WriteLine(item.ClosingPrice);
            }
           
            Assert.That(resultAsFloatList, Is.EquivalentTo(expectedResultAsFloatList));
            

        }


        [Test]
        public void Test2()
        {
            var expectedResult = new List<ArimaData> {
            new ArimaData(0,     new DateTime()),
            new ArimaData(1.234f,new DateTime()),
            new ArimaData(1.236f,new DateTime()),
            new ArimaData(1.244f,new DateTime()),
            new ArimaData(1.222f,new DateTime()),
            new ArimaData(1.88f, new DateTime()),
            new ArimaData(1.244f,new DateTime()),
            new ArimaData(1.24f, new DateTime()),
            new ArimaData(1.28f, new DateTime()),
            new ArimaData(1.004f,new DateTime()),
            new ArimaData(1.24f, new DateTime()),
            new ArimaData(1.00f, new DateTime()),
            new ArimaData(1.23f, new DateTime()),
            new ArimaData(1.20f, new DateTime()),
            new ArimaData(1.24f, new DateTime()),
            new ArimaData(1.234f,new DateTime()),
            //new ArimaData(1.284f, "Data")

        };




            var result = Arima.ShiftFloat(1, testData);

            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
            //List<float> resultAsFloatList = result.Select(item => item.ClosingPrice).ToList();
            List<double> expectedResultAsFloatList = expectedResult.Select(item => item.ClosingPrice).ToList();
            //foreach (var item in result)
            //{
            //    Console.WriteLine(item.ClosingPrice);
            //}

            Assert.That(result, Is.EquivalentTo(expectedResultAsFloatList));
            Assert.AreEqual(result.Count, expectedResultAsFloatList.Count);


        }

        [Test]
        public void CreatingLaggedVectors() {
            List<List<float>> expectedResult = new List<List<float>> {
                new List<float> {0, 1.111f, 1.222f, 1.333f, 1.444f, 1.555f, 1.666f },
                new List<float> {0, 0, 1.111f, 1.222f, 1.333f, 1.444f, 1.555f }
            };
            var res = Arima.CreateLaggedVectors(2, laggedVectorsTestData);
            foreach (var item in res)
            {
                foreach (var item2 in item)
                {
                    Console.WriteLine(item2);
                }

            }
            Assert.That(res, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void SplittingLaggedVectors()
        {

            List<List<float>> expectedXTrain = new List<List<float>> {
                new List<float> {0, 1.111f, 1.222f, 1.333f, 1.444f },
                new List<float> {0, 0, 1.111f, 1.222f, 1.333f }
            };

            List<List<float>> expectedXTest = new List<List<float>> {
                new List<float> {1.555f, 1.666f },
                new List<float> {1.444f, 1.555f }
            };
            var laggedVectors = Arima.CreateLaggedVectors(2, laggedVectorsTestData);
            var splitedLaggedVectors = Arima.SplitLaggedVectors(laggedVectors, 5);
            foreach (var item in splitedLaggedVectors[0])
            {
                foreach (var item2 in item)
                {
                    Console.WriteLine(item2);
                }

            }
            Assert.That(splitedLaggedVectors[0], Is.EquivalentTo(expectedXTrain));
            Assert.That(splitedLaggedVectors[1], Is.EquivalentTo(expectedXTest));

            Assert.That(splitedLaggedVectors[0][0], Is.EquivalentTo(expectedXTrain[0]));
            Assert.That(splitedLaggedVectors[0][1], Is.EquivalentTo(expectedXTrain[1]));
        }

        [Test]
         public void DeletingZerosFromLaggedTrainData()
        {
            List<List<float>> expectedResult = new List<List<float>> {
                //new List<float> { 1.222f, 1.333f, 1.444f, 1.555f, 1.666f },
                //new List<float> {1.111f, 1.222f, 1.333f, 1.444f, 1.555f }
                 //new List<float> { 1.111f, 1.222f, 1.333f },
                 //new List<float> { 1.222f, 1.333f, 1.444f }
                new List<float> { 1.222f, 1.333f, 1.444f },
                new List<float> {1.111f, 1.222f, 1.333f }

            };
            var laggedVectors = Arima.CreateLaggedVectors(2, laggedVectorsTestData);
            var splitedLaggedVectorsTrain = Arima.SplitLaggedVectors(laggedVectors, 5)[0];
            var nonZeroLaggedVectors = Arima.DropZeros(2, splitedLaggedVectorsTrain);
            foreach (var item in nonZeroLaggedVectors)
            {
                foreach (var item2 in item)
                {
                    Console.WriteLine(item2);
                }

            }
            Assert.That(nonZeroLaggedVectors, Is.EquivalentTo(expectedResult));
            Assert.That(nonZeroLaggedVectors[0], Is.EquivalentTo(expectedResult[0]));
        }


        [Test]
        public void Test6()
        {
            List<List<float>> expectedXTrain = new List<List<float>> {
            
                 new List<float> { 1.222f, 1.333f, 1.444f },
                new List<float> { 1.111f, 1.222f, 1.333f }
            };

            List<List<float>> expectedXTest = new List<List<float>> {
                new List<float> {1.555f, 1.666f },
                new List<float> {1.444f, 1.555f }
            };

            List<float> expectedYTrain = new List<float> { 1.333f, 1.444f, 1.555f };
            List<float> expectedYTest = new List<float> { 1.666f, 1.777f };







            var result = Arima.SplitToTrainTestData(2, laggedVectorsTestData, 5);

            foreach (var item in result.yTrain)
            {
               
                    Console.WriteLine(item);
                

            }
            Assert.That(result.xTrain, Is.EquivalentTo(expectedXTrain));
            Assert.That(result.xTest, Is.EquivalentTo(expectedXTest));
            Assert.That(result.yTrain, Is.EquivalentTo(expectedYTrain));
            Assert.That(result.yTest, Is.EquivalentTo(expectedYTest));

            Assert.That(result.xTrain[0], Is.EquivalentTo(expectedXTrain[0]));
            Assert.That(result.xTrain[1], Is.EquivalentTo(expectedXTrain[1]));

        }

        [Test]
        public void  TestAutoRegression()
        {
            //var result = Arima.SplitToTrainTestData(2, laggedVectorsTestData, 5);
            var splitedDataSet = Arima.SplitToTrainTestData(4, laggedVectorsTestData, 4);

            var res = Arima.AutoRegression(2, laggedVectorsTestData,splitedDataSet);


            //Console.WriteLine(res);

            //foreach (var item in res)
           // {
            //    Console.WriteLine(item);
            //}
            //Console.WriteLine(res.Length);

            //------------------

            

            /*Console.WriteLine();

            var result = Arima.Transpose(splitedDataSet.xTrain);

            var y = Arima.Transpose(res);

            var dotP = Arima.DotProduct(result, y);

            Console.WriteLine("Result");
            foreach (var item in result)
            {
                Console.WriteLine($"{item[0]} {item[1]}");
            }

            Console.WriteLine("Y");
            foreach (var item in y)
            {
                Console.WriteLine(item[0]);
            }



            Console.WriteLine("Final");
            foreach (var item in dotP)
            {
                Console.WriteLine(item[0]);
            }*/

            

            


        }

        [Test]
        public void TestTransposition() {
            List<List<double>> expectedXTrain = new List<List<double>> {

                 new List<double> { 1.222, 1.333, 1.444 },
                new List<double> { 1.111, 1.222, 1.333 }
            };

            double[,] expectedXTrainAsTransposedArray = new double[3, 2] {
                { 1.222, 1.111 },  {1.333, 1.222 }, {1.444, 1.333 },
            };

            var splitedDataSet = Arima.SplitToTrainTestData(2, laggedVectorsTestData, 5);

            var result = Arima.Transpose(splitedDataSet.xTrain);

            foreach (var item in result)
            {
                Console.WriteLine( $"{item[0]},  {item[1]}");
            }

            Assert.AreEqual(Math.Round(result[0][0],3), expectedXTrainAsTransposedArray[0,0]);
            Assert.AreEqual(Math.Round(result[0][1], 3), expectedXTrainAsTransposedArray[0,1]);
            Assert.AreEqual(Math.Round(result[1][0], 3), expectedXTrainAsTransposedArray[1,0]);
            Assert.AreEqual(Math.Round(result[1][1], 3), expectedXTrainAsTransposedArray[1, 1]);
            Assert.AreEqual(Math.Round(result[2][0], 3), expectedXTrainAsTransposedArray[2, 0]);
            Assert.AreEqual(Math.Round(result[2][1], 3), expectedXTrainAsTransposedArray[2, 1]);


        }

        [Test]
        public void TestDotProduct()
        {
            //var result = Arima.SplitToTrainTestData(2, laggedVectorsTestData, 5);

            double[][] matrix1 = new double[][] {
                new double[]{1.222 ,1.111},
                new double[]{1.333 ,1.222},
                new double[]{1.444 ,1.333},
                new double[]{1.555 ,1.444},
            };

            double[][] matrix2 = new double[][] {
                 new double[]{ 1.04638783 },
                 new double[]{-0.04638783 }
            };

            var res = MatrixHelpers.DotProduct(matrix1, matrix2);
            //Console.WriteLine(res);

            foreach(var item in res)
            {
                Console.WriteLine(item[0]);
            }
            Console.WriteLine(res.Length);

            Console.WriteLine("-----------");

            foreach (var item in res)
            {
                Console.WriteLine(item[0] + 0.10585095057034244);
            }

        }

        [Test]
        public void Test8() {
            List<double> input = new List<double>() { 1, 3, 4, 7, 9 };
            var res = MatrixHelpers.CalculateOrderDiscreteDiffrence(input);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }
            
        }

        [Test]
        public void Test9()
        {
            List<double> input = new List<double>() { 1, 3, 4, 7, 9 };
            var res = MatrixHelpers.CalculateOrderDiscreteDiffrence(input,2);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }

        }

        [Test]
        public void Test10()
        {
            List<double> input = new List<double>() { 1,1, 2, 3, 5, 8 };
            var res = MatrixHelpers.CalculateDiffrence(input, 3);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }

        }

        [Test]
        public void Test11()
        {
            double[] arr1 = new double[] { 1, 1, 2, 3, 4, 5 };
            double[] arr2 = new double[]       { 2, 3, 4, 2 };
            var res = Arima.SumArraysWithDiffrentSize(arr1, arr2);
            double[] expectedRes = new double[] { 0, 0, 4, 6, 8, 7 };
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }

            Assert.AreEqual(expectedRes[0], res[0]);
            Assert.AreEqual(expectedRes[1], res[1]);
            Assert.AreEqual(expectedRes[2], res[2]);
            Assert.AreEqual(expectedRes[3], res[3]);
            Assert.AreEqual(expectedRes[4], res[4]);
            Assert.AreEqual(expectedRes[5], res[5]);

        }







        /*[Test]
        public void Test3()
        {
            var expectedYtrain = new List<float> { 1.111f,2.222f,3.333f,4.444f};
            var expectedYtest = new List<float> { 5.555f, 6.666f,7.777f };

            //var expected




            var result = Arima.ShiftFloat(1, testData);

            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
            //List<float> resultAsFloatList = result.Select(item => item.ClosingPrice).ToList();
            List<float> expectedResultAsFloatList = expectedResult.Select(item => item.ClosingPrice).ToList();
            //foreach (var item in result)
            //{
            //    Console.WriteLine(item.ClosingPrice);
            //}

            Assert.That(result, Is.EquivalentTo(expectedResultAsFloatList));
            Assert.AreEqual(result.Count, expectedResultAsFloatList.Count);


        }*/

    }
}
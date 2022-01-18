
using NUnit.Framework;
using MachineLearning.Trainers;
using System.Collections.Generic;
using System;
using System.Linq;
using MachineLearning.DataModels;

namespace MachineLearning.Tests
{
    class FullDataTests
    {
        List<ArimaData> inputData;


        [SetUp]
        public void Setup()
        {
            inputData = TestData.GetData();
        }

        [Test]
        public void TestDotProduct()
        {
            ArimaTrainer.Solve(14, 5, inputData);
            

        }

        [Test]
        public void TestDotProduct2()
        {
            ArimaTrainer.Solve(14, 2, inputData);


        }

        [Test]
        public void TestOnDbData()
        {
            var res = ArimaTrainer.Solve(14, 2, "AAPL", 30);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }


        }

        [Test]
        public void TestAutoCorrelation()
        {
            

            var r = ArimaTrainer.PrepareInputData(inputData);
            var data = r.Select(el => el.ClosingPrice);

            List<double> autoCorrelations = new List<double>(); 
            var ACF = MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(data, 2, x => x * x);

            for (int i = 0; i < 30; i++)
            {
                autoCorrelations.Add(MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(data, i, x => x * x));
            }

            foreach (var item in autoCorrelations)
            {
                Console.WriteLine(item);
            }


        }

        [Test]
        public void TestAutoCorrelationOnNonDiffrencedData()
        {


            //var r = Arima.PrepareInputData(inputData);
            var data = inputData.Select(el => el.ClosingPrice);

            List<double> autoCorrelations = new List<double>();
            var ACF = MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(data, 2, x => x * x);

            for (int i = 0; i < 30; i++)
            {
                autoCorrelations.Add(MathNet.Numerics.Statistics.Mcmc.MCMCDiagnostics.ACF(data, i, x => x * x));
            }

            foreach (var item in autoCorrelations)
            {
                Console.WriteLine(item);
            }


        }


    }
}

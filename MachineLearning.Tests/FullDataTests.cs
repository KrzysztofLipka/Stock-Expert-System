
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
            Arima.Solve(14, 5, inputData);
            

        }

        [Test]
        public void TestDotProduct2()
        {
            Arima.Solve(14, 2, inputData);


        }

        [Test]
        public void TestOnDbData()
        {
            var res = Arima.Solve(14, 2, "AAPL", 30);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }


        }
    }
}


using NUnit.Framework;
using MachineLearning.Trainers;
using System.Collections.Generic;
using System;
using System.Linq;

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
    }
}

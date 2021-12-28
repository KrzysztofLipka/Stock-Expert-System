
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
            Arima.Solve(7, 12, inputData);
            

        }
    }
}

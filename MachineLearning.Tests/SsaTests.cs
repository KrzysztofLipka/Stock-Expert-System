using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using MachineLearning.Trainers;
using System.Collections.Generic;
using System;
using System.Linq;
using MachineLearning.DataModels;

namespace MachineLearning.Tests
{
   
    
    class SsaTests
    {
        ForecastBySsa ssa;
        [SetUp]
        public void Setup()
        {
            ssa = new ForecastBySsa();
        }

       
        [Test]
        public void TestDotProduct()
        {
            var result = ssa.Predict("AAPL", 30,new DateTime(), true);

            foreach (var item in result.Result)
            {
                Console.WriteLine(item);
            }


        }
    }
}

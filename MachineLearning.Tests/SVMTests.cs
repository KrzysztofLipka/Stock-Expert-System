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
    public class SVMTests
    {
        BinaryClassifier binary;

        [SetUp]
        public void Setup()
        {
            binary = new BinaryClassifier();
        }

        [Test]
        public void TestDefault()
        {
            binary.Predict("1", "2", "nke", "^dji", 2000 ,30,30);
        }

        [Test]
        public void TestDefault2()
        {
            binary.Predict("1", "2", "AAPL", "^ndq", 2000 ,30,30);
        }

        [Test]
        public void TestDefault3()
        {
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 5, 5);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 30, 5);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 270, 5);

            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 5, 30);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 30, 30);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 270, 30);

            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 5, 270);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 30, 270);
            binary.Predict("1", "2", "AAPL", "^ndq", 2000, 270, 270);
        }

        [Test]
        public void TestDefault4()
        {
            //binary.Predict("1", "2", "nke", "^dji", 2000);
            binary.Predict("1", "2", "nke", "^dji", 2000, 5, 5);
            binary.Predict("1", "2", "nke", "^dji", 2000, 30, 5);
            binary.Predict("1", "2", "nke", "^dji", 2000, 270, 5);
                                              
            binary.Predict("1", "2", "nke", "^dji", 2000, 5, 30);
            binary.Predict("1", "2", "nke", "^dji", 2000, 30, 30);
            binary.Predict("1", "2", "nke", "^dji", 2000, 270, 30);
                                              
            binary.Predict("1", "2", "nke", "^dji", 2000, 5, 270);
            binary.Predict("1", "2", "nke", "^dji", 2000, 30, 270);
            binary.Predict("1", "2", "nke", "^dji", 2000, 270, 270);
        }

        [Test]
        public void TestDefault5()
        {
            binary.Predict("1", "2", "nke", "^dji", 2000, 270, 270);
        }

    }
}

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
        SSATrainSizeWindowSizeTrainer ssa;
        SSAWindowsSizeTrainer sSAWindowsSize;

        [SetUp]
        public void Setup()
        {
            ssa = new SSATrainSizeWindowSizeTrainer();
            sSAWindowsSize = new SSAWindowsSizeTrainer();
        }

       
        [Test]
        public void TestDefault()
        {
            var result = ssa.Forcast("AAPL", 30,new DateTime());

            foreach (var item in result.Result)
            {
                Console.WriteLine(item);
            }


        }

        

        [Test]
        public void TestSolve2()
        {
            DateTime maxDate = new DateTime(2020, 11, 11);
            DateTime maxDate2 = new DateTime(2019, 3, 11);
            var result = ssa.SolveAndPlot(
                maxDate, 
                "AAPL", 
                400, 
                30, 
                32, 
                100, 
                100, 
                "secondTest1");

            var result2 = ssa.SolveAndPlot(
                maxDate, 
                "AAPL", 
                400, 
                30, //horizon 
                100/2-1, // window L
                100, //trainsize
                100, //seriesLength N
                "rys1");


            var result3 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                100, //trainsize
                100, //seriesLength N
                "rys2");

            var result4 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                100, //trainsize
                34, //seriesLength N
                "rys3");

            var result5 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                400, //trainsize
                100, //seriesLength N
                "rys4");

            //var result3 = ssa.SolveAndPlot(maxDate2, "AAPL", 400, 30, 32, 100, 100, "thirdTest");

            //var result4 = ssa.SolveAndPlot(maxDate2, "AAPL", 401, 30, 32, 100, 100, "fourthTest");






        }

        [Test]
        public void TestSolve()
        {
            DateTime maxDate = new DateTime(2020, 11, 11);
            DateTime maxDate2 = new DateTime(2019, 6, 11);
            var result = ssa.SolveAndPlot(maxDate, "AAPL", 400, 30, 100/3, 100, 100, "rys5");

            //var result2 = ssa.SolveAndPlot(maxDate, "AAPL", 400, 30, 32, 100, 100, "rys6");

            var result3 = ssa.SolveAndPlot(maxDate2, "AAPL", 400, 30, 100/3, 100, 100, "rys6");

            var result4 = ssa.SolveAndPlot(maxDate2, "AAPL", 400, 30, 100 / 3, 200, 100, "rys7");

            var result5 = ssa.SolveAndPlot(maxDate2, "AAPL", 400, 30, 100 / 3+1, 100, 100, "rys8");

            //var result4 = ssa.SolveAndPlot(maxDate2, "AAPL", 401, 30, 32, 100, 100, "rys8");






        }

        [Test]
        public void TestSolve3()
        {
            DateTime maxDate = new DateTime(2019, 4, 17);
            DateTime maxDate2 = new DateTime(2019, 3, 11);
            var result = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30,
                32,
                100,
                100,
                "secondTest11");

            var result2 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 2 - 1, // window L
                100, //trainsize
                100, //seriesLength N
                "secondTest12");


            var result3 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                100, //trainsize
                100, //seriesLength N
                "secondTest13");

            var result4 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                400, //trainsize
                400, //seriesLength N
                "secondTest14");

            var result5 = ssa.SolveAndPlot(
                maxDate,
                "AAPL",
                400,
                30, //horizon 
                100 / 3, // window L
                100, //trainsize
                400, //seriesLength N
                "secondTest15");

            //var result3 = ssa.SolveAndPlot(maxDate2, "AAPL", 400, 30, 32, 100, 100, "thirdTest");

            //var result4 = ssa.SolveAndPlot(maxDate2, "AAPL", 401, 30, 32, 100, 100, "fourthTest");






        }

        [Test]
        public void TestPredict()
        {
            ForecastBySsaParams parameters = new ForecastBySsaParams() {
                Horizon= 30,
            };
            DateTime maxDate = new DateTime(2020, 11, 11);
            DateTime maxDate2 = new DateTime(2019, 6, 11);
            //var result = ssa.SolveAndPlot(maxDate, "AAPL", 400, 30, 100 / 3, 100, 100, "rys9");
            var res = ssa.Forcast("", "", "AAPL", parameters, 500, false, maxDate, true, "rys9");
        }

        [Test]
        public void TestPredict2()
        {
            ForecastBySsaParams parameters = new ForecastBySsaParams()
            {
                Horizon = 30,
                WindowSize = 120
            };
            DateTime maxDate = new DateTime(2020, 11, 11);
            DateTime maxDate2 = new DateTime(2019, 6, 11);
            //var result = ssa.SolveAndPlot(maxDate, "AAPL", 400, 30, 100 / 3, 100, 100, "rys9");
            var res = sSAWindowsSize.Forcast("", "", "AAPL", parameters, 500, false, maxDate, true, "rys10");
        }


    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockExpertSystemBackend.Data;
using StockExpertSystemBackend.Data.Models;
using StockExpertSystemBackend.Utils;
//using Microsoft.Extensions.ML;
using Microsoft.ML.Transforms.TimeSeries;
using MachineLearning.DataModels;
using MachineLearning.Trainers;
using MachineLearning.DataLoading;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;
using Shared;

namespace StockExpertSystemBackend.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionsController : ControllerBase
    {
        
        [HttpPost]
        public ActionResult<PredictionResponse> Predict(PredictionRequest request)
        {
            var horizon = ParseRangeToHorizon(request.Range);

           
            if (request.PredictionModel.Contains("SSA"))
            {
                try
                {

                    SSATrainerBase ssa = SSATrainerFactory(request.PredictionModel);
                    SsaForecastOutput res = ssa.Forcast(request.Ticker, horizon, request.StartDate);
                    //ssa.Predict(request.Ticker, horizon, request.StartDate);
                    List<DateTime> dates = DateUtils.EachCalendarDayInRange(request.StartDate, horizon);
                    List<PredictionPoint> predictions = res.Result.Zip(dates, (result, date) => new PredictionPoint()
                    {
                        PredictedPrice = (decimal)result,
                        Date = date
                    }).ToList();

                    var dataLoader = new DbDataLoader();
                    var actualPrices = dataLoader.LoadDataFromDbFromMinDate(
                       request.Ticker,
                       request.StartDate,
                       horizon);

                    predictions = predictions.Zip(actualPrices, (predictions, actualPrice) => new PredictionPoint()
                    {
                        PredictedPrice = predictions.PredictedPrice,
                        ActualPrice = (decimal)actualPrice.ClosingPrice,
                        Date = predictions.Date
                    }).ToList();

                    return new PredictionResponse()
                    {
                        Predictions = predictions,
                        Id = new DateTime().Ticks.ToString(),
                        Ticker = request.Ticker
                    };
                }
                catch (Exception e)
                {
                    throw;
                }

            }

            else if (request.PredictionModel == "ARIMA")
            {
                try {
                    IEnumerable<double> res  = ArimaTrainer.Solve(10,5, request.Ticker, horizon, request.StartDate, true);
                    List<DateTime> dates = DateUtils.EachCalendarDayInRange(request.StartDate, horizon);
                    List<PredictionPoint> predictions = res.Zip(dates, (result, date) => new PredictionPoint()
                    {
                        PredictedPrice = (decimal)result,
                        Date = date
                    }).ToList();

                    var dataLoader = new DbDataLoader();
                    var actualPrices = dataLoader.LoadDataFromDbFromMinDate(
                        request.Ticker,
                        request.StartDate,
                        horizon);

                    predictions =  predictions.Zip(actualPrices, (predictions, actualPrice) => new PredictionPoint()
                    {
                        PredictedPrice = predictions.PredictedPrice,
                        ActualPrice = (decimal)actualPrice.ClosingPrice,
                        Date = predictions.Date
                    }).ToList();

                    return new PredictionResponse()
                    {
                        Predictions = predictions,
                        Id = new DateTime().Ticks.ToString(),
                        Ticker = request.Ticker
                    };


                } catch (Exception e) {
                    throw;
                }
            }

           
            else
            {
                throw new Exception("todo");
            }
        }

        /*[HttpGet("historicalDetails/{predictionId}")]
        public ActionResult<HistoricalPredictionDetailsResponse> GetHistoricalPredictionDetails(string predictionId)
        {
            return new HistoricalPredictionDetailsResponse()
            {
                Ticker = "AAPL",
                Quotes = new List<PredictionPoint> {
                    new PredictionPoint()
                    {
                        ActualPrice =  12.16M,
                        Date = new DateTime(2020, 11,12)
                    },
                    new PredictionPoint()
                    {
                        ActualPrice =  13.16M,
                        Date = new DateTime(2020, 11,13)
                    },
                    new PredictionPoint()
                    {
                        ActualPrice =  10.16M,
                        Date = new DateTime(2020, 11,14)
                    }
                },
                Id = "12345"
            };
        }*/

        [HttpGet("historical")]
        public IEnumerable<HistoricalPrediction> GetHistoricalPredictions()
        {
            try
            {
                var dataLoader = new DbDataLoader();
                var data = dataLoader.LoadHistoricalPredictions();
                return data.ToArray();
            }
            catch(Exception e) {
                throw e;
            }
        }

        private int ParseRangeToHorizon(string range) {
            if (range == "1 day") {
                return 1;
            }
            else if (range == "1 week")
            {
                return 5;
            }
            else if (range == "1 month")
            {
                return 30;
            }
            else if (range == "1 year")
            {
                return 365;
            }
            else 
            {
                return 1;
            }
        }

        private SSATrainerBase SSATrainerFactory(string trainerName) {
            if (trainerName.Equals("SSA (WindowsSize + TrainSize)")) {
                return new SSATrainSizeWindowSizeTrainer();
            }

            else if (trainerName.Equals("SSA (WindowsSize)"))
            {
                return new SSAWindowsSizeTrainer();
            }

            else if (trainerName.Equals("SSA (Constant Parameters)"))
            {
                return new SSAConstantParametersTrainer();
            }

            else
            {

                throw new Exception("Wrong parameters");
            }
        }


        /*[HttpGet("getPredictionVisualization")]
        public IEnumerable<HistoricalPredictionsResponse> GetHistoricalPredictions()
        {
            return new List<HistoricalPredictionsResponse>()
            {
                new HistoricalPredictionsResponse(){
                    Ticker = "AAPL",
                    Status = "Pending",
                    StartDate  = new DateTime(2020, 11,24),
                    EndDate = new DateTime(2020, 11,28),
                    Id = "12345"
                }

            };
        }*/



    }

    


}

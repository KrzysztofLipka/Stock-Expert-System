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
using StockExpertSystemBackendML.Model;
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

            if (request.PredictionModel == "Forecasting")
            {
                return GetForecastPrediction(request.Ticker, 30);
            }

            else if (request.PredictionModel == "SSA")
            {
                try
                {
                    ForecastBySsa ssa = new ForecastBySsa();
                    SsaForecastOutput res = ssa.Predict(request.Ticker, horizon, request.StartDate,true);
                    List<DateTime> dates = DateUtils.EachCalendarDayInRange(request.StartDate, horizon);
                    List<PredictionPoint> predictions = res.Result.Zip(dates, (result, date) => new PredictionPoint()
                    {
                        PredictedPrice = (decimal)result,
                        Date = date
                    }).ToList();

                    var actualPrices = DbDataLoader.LoadDataFromDbFromMinDate(
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
                    IEnumerable<double> res  = Arima.Solve(10,5, request.Ticker, horizon, request.StartDate, true);
                    List<DateTime> dates = DateUtils.EachCalendarDayInRange(request.StartDate, horizon);
                    List<PredictionPoint> predictions = res.Zip(dates, (result, date) => new PredictionPoint()
                    {
                        PredictedPrice = (decimal)result,
                        Date = date
                    }).ToList();

                    var actualPrices = DbDataLoader.LoadDataFromDbFromMinDate(
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

            else if (request.PredictionModel == "LbfgsPoissonRegression")
            {
                return getLbfsgRegression();
            }
            else
            {
                var input = new ModelInput();
                input.Col0 = "11.11.2012 00:00:00";
                //ModelOutput output = ConsumeModel.Predict(input);

                var dates = DateUtils.EachCalendarDayInRange(request.StartDate, DateUtils.ConvertDateRangeToNumberOfDays(request.Range));

                var res = new PredictionResponse()
                {
                    Ticker = request.Ticker,
                    Predictions = new List<PredictionPoint>(0)
                };

                foreach (var date in dates)
                {
                    var input1 = new ModelInput();
                    input1.Col0 = date.ToString("MM.dd.yyyy HH:mm:ss");
                    ModelOutput output1 = ConsumeModel.Predict(input1);
                    res.Predictions.Add(new PredictionPoint()
                    {
                        PredictedPrice = (decimal)output1.Score,
                        Date = date
                    });
                }

                return res;
            }
        }

        

        private PredictionResponse GetForecastPrediction(string ticker, int horizon) {
            MLContext ml = new MLContext();
            ITransformer model;

            using (var file = System.IO.File.OpenRead("../../../../MLModels/SSA_rds-a_30_12.11.2021.zip"))
                model = ml.Model.Load(file, out DataViewSchema schema);

            var engine = model.CreateTimeSeriesEngine<StockDataPointInput, NbpForecastOutput>(ml);

            var ress = engine.Predict();
            Console.WriteLine(ress);

            //var questions = _dataRepository.GetQuestions();
            var res  = new PredictionResponse()
            {
                Ticker = ticker,
                Predictions = new List<PredictionPoint> {
                    /*new PredictionPoint()
                    {
                        PredictedPrice =  (decimal)ress.Forecast[0],
                        Date = new DateTime(2020, 11,12)
                    },
                    new PredictionPoint()
                    {
                        PredictedPrice =  (decimal)ress.Forecast[1],
                        Date = new DateTime(2020, 11,13)
                    },
                    new PredictionPoint()
                    {
                        PredictedPrice =  (decimal)ress.Forecast[2],
                        Date = new DateTime(2020, 11,14)
                    },
                    new PredictionPoint()
                    {
                        PredictedPrice =  (decimal)ress.Forecast[3],
                        Date = new DateTime(2020, 11,15)
                    }*/

                }
            };

            DateTime timeSeriesStartDate = new DateTime(2020, 11, 12);

            int dateCounter = 1;

            foreach (var forecast in ress.Forecast)
            {
                res.Predictions.Add(new PredictionPoint()
                {
                    PredictedPrice = (decimal)forecast,
                    Date = timeSeriesStartDate.AddDays(dateCounter++)
                });
            }

            return res;
        }

        private PredictionResponse getLbfsgRegression() {
            MLContext ml = new MLContext();
            ITransformer model;

            using (var file = System.IO.File.OpenRead("../../../../MLModels/LbfgsPoissonRegression.zip"))
                model = ml.Model.Load(file, out DataViewSchema schema);

            var predictionEngine = ml.Model.CreatePredictionEngine<NbpDataRaw, NbpForecastOutput>(model);

            var t = new NbpDataRaw { Date = "2021-08-28" };
            var res = predictionEngine.Predict(t);

            return new PredictionResponse()
            {
                Ticker = "AALP",
                Predictions = new List<PredictionPoint> {
                    new PredictionPoint()
                    {
                        PredictedPrice =  (decimal)res.Forecast[0],
                        Date = new DateTime(2020, 11,12)
                    }
                }

            };




        }

        [HttpGet("historicalDetails/{predictionId}")]
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
        }

        [HttpGet("historical")]
        public IEnumerable<HistoricalPrediction> GetHistoricalPredictions()
        {
            var data = DbDataLoader.LoadHistoricalPredictions();
            /*return new List<HistoricalPredictionsResponse>()
            {
                new HistoricalPredictionsResponse(){
                    CompanyName = "AAPL",
                    Status = "Pending",
                    StartDate  = new DateTime(2020, 11,24),
                    EndDate = new DateTime(2020, 11,28),
                    Id = "12345"
                }
               
            };*/

            return data.ToArray();
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

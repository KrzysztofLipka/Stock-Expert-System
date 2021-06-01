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
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;
using StockExpertSystemBackendML.Model;

namespace StockExpertSystemBackend.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionsController : ControllerBase
    {
        //private readonly PredictionEnginePool<NbpData, NbpForecastOutput> _predictionEnginePool;
        //public PredictionsController(PredictionEnginePool<NbpData, NbpForecastOutput> predictionEnginePool)
        //{
        //    this._predictionEnginePool = predictionEnginePool;
        //}

        [HttpPost]
        public ActionResult<PredictionResponse> Predict(PredictionRequest request)
        {
            if (request.PredictionModel == "Forecasting")
            {
                return GetForecastPrediction(request.Ticker);
            }
            else {
                var input = new ModelInput();
                input.Col0 = "11.11.2012 00:00:00";
                //ModelOutput output = ConsumeModel.Predict(input);

                var dates = DateUtils.EachCalendarDayInRange(request.StartDate, DateUtils.ConvertDateRangeToNumberOfDays(request.Range));

                var res = new PredictionResponse() {
                    Ticker = request.Ticker,
                    Predictions = new List<PredictionPoint>(0)
                };

                foreach (var date in dates) {
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

        private PredictionResponse GetForecastPrediction(string ticker) {
            MLContext ml = new MLContext();
            ITransformer model;

            using (var file = System.IO.File.OpenRead("../../../../MLModels/forecast_model.zip"))
                model = ml.Model.Load(file, out DataViewSchema schema);

            var engine = model.CreateTimeSeriesEngine<NbpData,
                NbpForecastOutput>(ml);

            var ress = engine.Predict();
            Console.WriteLine(ress);

            //var questions = _dataRepository.GetQuestions();
            return new PredictionResponse()
            {
                Ticker = ticker,
                Predictions = new List<PredictionPoint> {
                    new PredictionPoint()
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
        }

    }

    


}

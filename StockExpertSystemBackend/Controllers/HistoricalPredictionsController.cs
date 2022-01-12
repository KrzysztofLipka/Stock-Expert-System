﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using DataSerializer;

namespace StockExpertSystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricalPredictionsController
    {
        [HttpPost]
        public ActionResult<int> SavePrediction(HistoricalPredictionRequest request)
        {
            DataRepository repo = new DataRepository();
            //var dbRequest = new HistoricalPrediction
            try
            {
                request.StartDate = DateTime.Parse(request.StartDateAsString);
                request.EndDate = DateTime.Parse(request.EndDateAsString);
                var result = repo.PostPrediction(request);
                return result;
            }
            catch(Exception e) {
                throw;
            }
            
        }

        [HttpPost("clearAll")]
        public void ClearPredictions()
        {
            DataRepository repo = new DataRepository();
            //var dbRequest = new HistoricalPrediction
            try
            {
                
                repo.CleanPredictions();
               
            }
            catch (Exception e)
            {
                throw;
            }

        }

        [HttpGet("historical")]
        public ActionResult<IEnumerable<HistoricalPrediction>> GetHistoricalPredictions()
        {
            try {
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
            } catch(Exception e) {
                throw;
            }
            
        }
    }
}

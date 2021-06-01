using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class PredictionResponse
    {
        public string Ticker { get; set; }

        public List<PredictionPoint> Predictions { get; set; }

        public string Id { get; set; }
    }
}

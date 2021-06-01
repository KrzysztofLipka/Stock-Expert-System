using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class HistoricalPredictionDetailsResponse
    {
        public string Ticker { get; set; }
        public string Id { get; set; }
        public List<PredictionPoint> Quotes { get; set; }
    }
}

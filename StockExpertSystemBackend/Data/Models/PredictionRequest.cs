using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class PredictionRequest
    {
        public string Ticker { get; set; }
        public DateTime StartDate { get; set; }
        public string Range { get; set; }

        public string PredictionModel { get; set; }
    }
}

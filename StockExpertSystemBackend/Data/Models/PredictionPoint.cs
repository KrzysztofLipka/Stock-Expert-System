using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class PredictionPoint
    {
        public decimal PredictedPrice { get; set; }

        public decimal? ActualPrice { get; set; }

        public DateTime Date { get; set; }
    }
}

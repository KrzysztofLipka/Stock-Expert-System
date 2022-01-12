using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class HistoricalPrediction
    {
        public string CompanyName { get; set; }
        //public string Status { get; set; }
        public float PredictedBuyPrice { get; set; }
        public float PredictedSellPrice { get; set; }
        public float ActualBuyPrice { get; set; }
        public float ActualSellPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
       
        public int PredictionId { get; set; }
    }

    public class HistoricalPredictionRequest : HistoricalPrediction {
        public string StartDateAsString { get; set; }
        public string EndDateAsString { get; set; }

    }
}

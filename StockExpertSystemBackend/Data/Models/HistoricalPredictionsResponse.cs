using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class HistoricalPredictionsResponse
    {

        public string CompanyName { get; set; }
        //public string Status { get; set; }
        public decimal PredictedBuyPrice { get; set; }
        public decimal PredictedSellPrice { get; set; }
        public decimal ActualBuyPrice { get; set; }
        public decimal ActualSellPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Id { get; set; }
    }
}

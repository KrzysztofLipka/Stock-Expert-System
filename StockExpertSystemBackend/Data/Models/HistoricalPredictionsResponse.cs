using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockExpertSystemBackend.Data.Models
{
    public class HistoricalPredictionsResponse
    {
        public string Ticker { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Id { get; set; }
    }
}

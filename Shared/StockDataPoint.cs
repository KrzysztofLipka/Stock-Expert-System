using System;

namespace Shared
{
    public class StockDataPoint
    {
        public decimal OpeningPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public long Volume { get; set; }
        public DateTime Time { get; set; }
        public string CompanyName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Externals.Data
{
    public class StockDataPointDb
    {
        public decimal OpeningPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public long Volume { get; set; }
        public DateTime Time { get; set; }
        public int CompanyId { get; set; }
    }
}

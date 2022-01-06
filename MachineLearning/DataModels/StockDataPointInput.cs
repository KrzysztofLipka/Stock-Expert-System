using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace MachineLearning.DataModels
{
    public class StockDataPointInput
    {
        [LoadColumn(0)]
        [ColumnName("ClosingPrice")]
        public float ClosingPrice { get; set; }

        [LoadColumn(1)]
        [ColumnName("PriceDate")]
        public DateTime Date { get; set; }
    }
}

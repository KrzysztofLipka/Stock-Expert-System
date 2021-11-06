using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace MachineLearning.DataModels
{
    class StockDataPointInput
    {
        //[LoadColumn(0)]
        //public string Date { get; set; } 

        [LoadColumn(0)]
        [ColumnName("ClosingPrice")]
        public float ClosingPrice { get; set; }
    }
}

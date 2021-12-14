using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace MachineLearning.DataModels
{
    public class StockDataPointBinaryInput
    {
        [LoadColumn(0)]
        [ColumnName("Label")]
        public bool IsRising { get; set; }

        public DateTime Date { get; set; }

        [LoadColumn(1)]
        [ColumnName("Features")]
        [VectorType(4)]
        public float[] Features { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace MachineLearning.DataModels
{
    public class NbpDataRaw
    {
        [LoadColumn(0)]
        public string Date { get; set; }

        [LoadColumn(1)]
        ///[ColumnName("Label")]
        public float ExchangeRate { get; set; }
    }
}
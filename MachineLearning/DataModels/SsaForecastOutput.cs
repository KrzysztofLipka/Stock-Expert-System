using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.DataModels
{
    public class SsaForecastOutput
    {
        public double [] Result { get; set; }
        public double Mae { get; set; }
        public double Rmse { get; set; }
        public double Acf { get; set; }
    }
}

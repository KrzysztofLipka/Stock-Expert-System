using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.DataModels
{
    public class ArimaData
    {
        public ArimaData(double closingPrice, DateTime date)
        {
            this.ClosingPrice = closingPrice;
            this.Date = date;
        }

        public ArimaData(DateTime date, double closingPrice)
        {
            this.ClosingPrice = closingPrice;
            this.Date = date;
        }

        public ArimaData() { }
        public double ClosingPrice;
        public DateTime Date;
    }
}

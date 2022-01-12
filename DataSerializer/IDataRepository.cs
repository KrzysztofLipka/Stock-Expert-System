using System;
using System.Collections.Generic;
using System.Text;
using Externals.Data;

using Shared;

namespace DataSerializer
{
    interface IDataRepository
    {
        IEnumerable<StockDataPoint> GetStockDataPoints(string StockName);

        void PostStockDataPoint(StockDataPoint dataPoint);

        int PostPrediction(HistoricalPrediction prediction);

        void PostManyStockDataPoints(IEnumerable<StockDataPointDb> dataPoint);
    }
}

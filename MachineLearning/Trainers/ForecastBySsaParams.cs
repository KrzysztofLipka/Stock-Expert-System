using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLearning.Trainers
{
    public class ForecastBySsaParams
    {
        public string OutputColumnName;
        public string InputColumnName;
        public int WindowSize;
        public int SeriesLength;
        public int TrainSize;
        public int Horizon;
        public bool IsAdaptive = false;
        public float DiscountFactor = 1;
        //RankSelectionMethod rankSelectionMethod = RankSelectionMethod.Exact;
        public int? Rank = null;
        public int? MaxRank = null;
        public bool ShouldStabilize = true;
        public bool ShouldMaintainInfo = false;
        //GrowthRatio? maxGrowth = null;
        public string ConfidenceLowerBoundColumn = null;
        public string ConfidenceUpperBoundColumn = null;
        public float confidenceLevel = 0.95F;
        public bool VariableHorizon = false;

        //public ForecastBySsaParams(int numberOfRows)
        //{
        //    this.TrainSize = numberOfRows;
        //    this.SeriesLength = 365;
        //    this.Horizon = 30;
        //    this.WindowSize = 30;
        //
        //}
    }
}

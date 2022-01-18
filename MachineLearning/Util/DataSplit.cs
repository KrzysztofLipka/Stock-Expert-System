using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using MachineLearning.DataModels;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using ScottPlot;
using MachineLearning.DataLoading;


namespace MachineLearning.Util
{
    public static class DataSplit
    {
        //todo
        public static void SplitList<T>(
            List<T> inputList,
            out List<T> listOne,
            out List<T> listTwo,
            int splitIndex
            )
        {
            var res1 = new List<T>();
            var res2 = new List<T>();

            for (int i = 0; i < inputList.Count; i++)
            {
                if (i < splitIndex)
                {
                    res1.Add(inputList[i]);
                }
                else
                {
                    res2.Add(inputList[i]);
                };

            }

            listOne = res1;
            listTwo = res2;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using MachineLearning.DataModels;
using System.Linq;

namespace MachineLearning.Util
{
    public class BinaryConverter
    {
        static int daysAhead = 100;
        public static List<StockDataPointBinaryInput> Convert(IEnumerable<StockDataPointInput> input, IEnumerable<StockDataPointInput> indexInput) {
            StockDataPointInput previous = null;
            List<StockDataPointBinaryInput> result = new List<StockDataPointBinaryInput>();
            int numberOfDays = 5;
           

            float counter = 0;
            List<float> inputAsFloatList = new List<float>();
            List<float> inputAsFloatListIndex = new List<float>();

            foreach (var inputItem in input)
            {
                inputAsFloatList.Add(inputItem.ClosingPrice);
            }

            foreach (var inputItemIndex in indexInput)
            {
                inputAsFloatListIndex.Add(inputItemIndex.ClosingPrice);
            }

            List<float> momentum = CalculateMomentum(numberOfDays, inputAsFloatList);
            List<bool> isRising = IsRising(numberOfDays, inputAsFloatList);
            List<float> votality = CalculateVoliatility(numberOfDays, inputAsFloatList);



            List<float> momentumIndex = CalculateMomentum(numberOfDays, inputAsFloatListIndex);
            List<float> votalityIndex = CalculateVoliatility(numberOfDays, inputAsFloatListIndex);

            for (int i = 0; i < momentum.Count(); i++)
            {
                float[] features = new float[4] { momentum[i], votality[i], momentumIndex[i], votalityIndex[i] };
                result.Add(new StockDataPointBinaryInput() {
                Features = features,
                IsRising = isRising[i]
                });

            }

            return result;

            
            

            /*foreach (var point in input)
            {
                if (previous is null) {
                    previous = point;
                    continue;
                };

                var d = point.Date;

                result.Add(
                    new StockDataPointBinaryInput() {
                        Date = point.Date,
                        //Date = new float[1] { (float)point.Date.Ticks },
                        IsRising = point.ClosingPrice > previous.ClosingPrice
                    }
                    );
                previous = point;
            }
             return result;  */        
        }

        private static List<float> CalculateMomentum(int numberOfDays, List<float> priceArray)
        {
            //int daysAhead = 270;
            List<float> momentumList = new List<float>();
            List<int> movingMomentumList = new List<int>();
            for (int i = 1; i < numberOfDays + 1; i++)
            {
                movingMomentumList.Add(priceArray[i] > priceArray[i - 1] ? 1 : -1);
            }

            momentumList.Add((float)movingMomentumList.Average());

            for (int j = numberOfDays + 1; j < priceArray.Count - daysAhead; j++)
            {
                movingMomentumList.RemoveAt(0);
                movingMomentumList.Add(priceArray[j] > priceArray[j - 1] ? 1 : -1);
                momentumList.Add((float)movingMomentumList.Average());
            }

            return momentumList;

        }

        private static List<float> CalculateVoliatility(int numberOfDays, List<float> priceArray) {
            //int daysAhead = 270;
            List<float> voliatilityList = new List<float>();
            List<float> movingVoliatilityList = new List<float>();

            for (int i = 1; i < numberOfDays + 1; i++)
            {
                movingVoliatilityList.Add(100*(priceArray[i] - priceArray[i - 1]) / priceArray[i - 1]);
            }

            voliatilityList.Add((float)movingVoliatilityList.Average());


            for (int j = numberOfDays + 1; j < priceArray.Count - daysAhead; j++)
            {
                movingVoliatilityList.RemoveAt(0);
                movingVoliatilityList.Add(100 * (priceArray[j] - priceArray[j - 1]) / priceArray[j - 1]);
                voliatilityList.Add((float)movingVoliatilityList.Average());
            }

            return voliatilityList;



        }

        private static List<bool> IsRising(int numberOfDays, List<float> priceArray) {
            //int daysAhead = 270;
            List<bool> isRising = new List<bool>();
            for (int i = numberOfDays; i < priceArray.Count() - daysAhead; i++)
            {
                var t = priceArray[i + daysAhead];
                var t2 = priceArray[i];
                isRising.Add(priceArray[i+ daysAhead] > priceArray[i]);
            }

            return isRising;
        }
    }
}

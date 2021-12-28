using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MachineLearning.Util
{
    public class MatrixHelpers
    {
        public static double[][] DotProduct(double[][] FirstInputMatrix, double[][] SecondInputMatrix)
        {
            return FirstInputMatrix.Select( // goes through <lhs> row by row
            (row, rowIndex) =>
            SecondInputMatrix[0].Select( // goes through first row of <rhs> cell by cell
                (_, columnIndex) =>
                SecondInputMatrix.Select(__ => __[columnIndex]) // selects column from <rhs>
                    .Zip(row, (rowCell, columnCell) => rowCell * columnCell).Sum() // does scalar product
                ).ToArray()
            ).ToArray();
        }

        public static string PrintRow(IEnumerable<double> row)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var el in row)
            {
                sb.Append($"{el}, ");
            }
            return sb.ToString();
        }

        public static string PrintRow(IEnumerable<float> row)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var el in row)
            {
                sb.Append($"{el}, ");
            }
            return sb.ToString();
        }

        public static List<float> CalculateOrderDiscreteDiffrence(IEnumerable<float> input) {
            List<float> inputAsList = input.ToList();
            List<float> result = new List<float>();
            for (int i = 0; i < input.Count() -1; i++)
            {
                result.Add(inputAsList[i + 1] - inputAsList[i]);
            }

            return result;
        }

        public static List<float> CalculateOrderDiscreteDiffrence(IEnumerable<float> input, int n) {
            //List<float> inputAsList = input.ToList();
            List<float> result = CalculateOrderDiscreteDiffrence(input);

            for (int i = 0; i < n-1; i++)
            {
                result = CalculateOrderDiscreteDiffrence(result);
            }

            return result;
        }

        public static List<double> CalculateOrderDiscreteDiffrence(IEnumerable<double> input)
        {
            List<double> inputAsList = input.ToList();
            List<double> result = new List<double>();
            for (int i = 0; i < input.Count() - 1; i++)
            {
                result.Add(inputAsList[i + 1] - inputAsList[i]);
            }

            return result;
        }

        public static List<double> CalculateOrderDiscreteDiffrence(IEnumerable<double> input, int n)
        {
            //List<float> inputAsList = input.ToList();
            List<double> result = CalculateOrderDiscreteDiffrence(input);

            for (int i = 0; i < n - 1; i++)
            {
                result = CalculateOrderDiscreteDiffrence(result);
            }

            return result;
        }

        public static List<double> CalculateDiffrence(List<double> input, int n)
        {
            List<double> inputAsList = input.ToList();
            List<double> result = new List<double>();

            for (int i = 0; i < input.Count(); i++)
            {
                if (i < n )
                {
                    result.Add(0);
                    //continue;
                }

                else {
                    result.Add(inputAsList[i] - inputAsList[i-n]);
                }
            }

            return result;
        }

        public static List<float> CalculateDiffrence(List<float> input, int n)
        {
            List<float> inputAsList = input.ToList();
            List<float> result = new List<float>();

            for (int i = 0; i < input.Count(); i++)
            {
                if (i < n)
                {
                    result.Add(0);
                    //continue;
                }

                else
                {
                    result.Add(inputAsList[i] - inputAsList[i - n]);
                }
            }

            return result;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis
{
    public static class Util
    {
        public static double Mean(double[] input)
        {
            return Mean(input, false);
        }
        public static double Mean(double[] input, bool ignoreZeros)
        {
            double sum = 0;
            double count = 0;
            for (int i = 0; i < input.Length; i++)
            {
                sum += input[i];
                count++;
            }
            return sum / count;
        }
        public static double[] Mean(double[][] input)
        {
            double[] means = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                means[i] = Mean(input[i]);
            }
            return means;
        }
        public static void GetMinMax(double[] input, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > max) { max = input[i]; }
                if (input[i] < min) { min = input[i]; }
            }
        }

        public static void GetMinMax(double[][] input, out double[] min, out double[] max)
        {
            min = new double[input.Length];
            max = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                GetMinMax(input[i], out min[i], out max[i]);
            }
        }

        public static int GetMaxLength(double[][] input)
        {
            int maxLength = int.MinValue;
            for (int i = 0; i < input.Length; i++)
            {
                if(input[i].Length > maxLength)
                {
                    maxLength = input[i].Length;
                }
            }
            return maxLength;
        }

        public static int Count(bool[] input)
        {
            int sum = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if(input[i])
                {
                    sum++;
                }
            }
            return sum;
        }
    }
}

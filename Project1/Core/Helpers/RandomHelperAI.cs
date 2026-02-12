using System;
using Project1.Core.AI;
using Project1.Framework.Helpers;

namespace Project1.Core.Helpers
{
    internal static class RandomHelperAI
    {
        static public double[] NextNormalsBalanced(int count)//, double min, double max, double sum)
        {
            var values = new double[count];
            double min = -1, max = 1;
            double sum = 0;
            for (int i = 0; i < count - 1; i++)
            {
                var rest = count - (i + 1);
                double restmin = min * rest;
                double restmax = max * rest;
                min = Math.Max(min, sum - restmax);
                max = Math.Min(max, sum - restmin);

                var v = RandomHelper.NextNormal(min, max);
                if (Math.Abs(v) > Trait.ValueRange)
                    throw new Exception();
                sum -= v;
                values[i] = v;
            }
            values[count - 1] = sum;
            return values;
        }
    }
}
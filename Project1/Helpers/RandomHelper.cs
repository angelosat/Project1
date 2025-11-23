using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class RandomHelper
    {
        static readonly Random rand = new Random(); //reuse this if you are generating many


        /// <summary>
        /// https://www.johndcook.com/blog/csharp_phi/
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static double Phi(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/218060/random-gaussian-variables
        /// https://stackoverflow.com/questions/1303368/how-to-generate-normally-distributed-random-from-an-integer-range
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        static public double NextGaussian(double mean = 0, double stdDev = 1)
        {
            //Random rand = new Random(); //reuse this if you are generating many
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
        /// <summary>
        /// Returns a number within (-1, 1) with gaussian distribution.
        /// </summary>
        /// <returns></returns>
        static public double NextNormal()
        {
            double g;
            do
            {
                g = RandomHelper.NextGaussian(0, 1/3d);
            } while (g > 1);
            return g;
        }
        static public double NextNormal(double min, double max)
        {
            if (Math.Abs(min) > 1 || Math.Abs(max) > 1)
                throw new Exception();
            double g;
            do
            {
                g = RandomHelper.NextGaussian(0, 1 / 3d);
            } while (g < min || g > max);
            return g;
        }
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

                var v = NextNormal(min, max);
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

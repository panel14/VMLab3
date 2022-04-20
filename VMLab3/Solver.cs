using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace VMLab3
{
    internal class Solver
    {
        private static Func<double, double> func1 = (x) => 2 * x + 3.0 / Math.Sqrt(x);
        private static Func<double, double> func2 = (x) => 1.0 / 12 * Math.Pow(x, 4) + 1.0 / 3 * x - 1.0 / 60;
        private static Func<double, double> func3 = (x) => Math.Sin(x) / x;
        private static Func<double, double> func4 = (x) => Math.Sin(1 / x);


        private static Func<double, double> dder1 = (x) => 3 * 3.0 / (4 * Math.Pow(x, 5.0/2));
        private static Func<double, double> dder2 = (x) => Math.Pow(x, 2);
        private static Func<double, double> dder3 = (x) => (-Math.Sin(x) - 2 *Math.Cos(x) / x + 2 * Math.Sin(x) / Math.Pow(x, 2)) / x;
        private static Func<double, double> dder4 = (x) => (2 * Math.Cos(1/x) - Math.Sin(1 / x)) / Math.Pow(x, 4);

        private static Func<double, double>[] funcs = { func1, func2, func3, func4 };
        private static Func<double, double>[] ders = { dder1, dder2, dder3, dder4 };

        private static double?[] results = new double?[] { null, null, null };
        private static double?[] errors = new double?[] { null, null, null };
        private static int[] iters = new int[3];
        private static double[] borders = new double[2];

        private static List<double> firstGaps = new List<double>();
        private static double secondGap = Double.NaN;

        private static double FixByAverage(double x, double eps, int funcNum)
        {
            double left = funcs[funcNum](x - eps);
            if (left == Double.NaN)
                left = 0;
            return (left + funcs[funcNum](x + eps)) / 2;
        }

        private static double[] FixByCut(double x, double eps, int funcNum)
        {
            return new double[] { funcs[funcNum](x - eps), funcs[funcNum](x + eps) };
        }

        private static int CheckPoint(double x, double step, int funcNum)
        {
            double test = funcs[funcNum](x);
            if (Double.IsInfinity(test) || Double.IsNaN(test))
            {
                double testL = funcs[funcNum](x - 0.0000001);
                double testR = funcs[funcNum](x + 0.0000001);

                double nextL = funcs[funcNum](x - step);
                double nextR = funcs[funcNum](x + step);

                bool nonLLimit = Double.IsInfinity(testL) || Double.IsNaN(testL) || Math.Abs(testL - nextL) > 10;
                bool nonRLimit = Double.IsInfinity(testR) || Double.IsNaN(testR) || Math.Abs(testR - nextR) > 10;

                if (nonLLimit || nonRLimit)
                {
                    secondGap = x;
                    return 2;
                }
                else
                {
                    if (!firstGaps.Contains(x))
                        firstGaps.Add(x);
                    return 1;
                }               

            }
            return 0;
        }
        
        private static double CalcError(double x, double h, int funcNum)
        {
            double error = Math.Abs(-1.0 / 12 * Math.Pow(h, 3) * ders[funcNum](x));
            return error;
        }

        public static void Solve(int funcNum, double[] section, int fixMethod)
        {
            Array.Fill(results, null);
            Array.Fill(errors, null);

            double eps = 0.001;
            int loopCount = 0;

            double gapShift = 0;
            bool isShift = false;


            for (int i = 2; i <= 4; i++)
            {

                double h = 1.0 / Math.Pow(10, i);
                List<double> xs = new List<double>();

                int iter = (int)((section[1] - section[0]) / h);

                int k = 0;
                double cur_val = section[0] - h;
                while (k < iter)
                {
                    cur_val = Math.Round(cur_val + h, i);
                    if (Double.IsNaN(funcs[funcNum](cur_val)))
                    {
                        if (CheckPoint(cur_val, h, funcNum) == 1)
                        {
                            xs.Add(cur_val);
                            k++;
                        }
                        else
                            iter--;
                    } 
                    else
                    {
                        xs.Add(cur_val);
                        k++;
                    }
                }
                iters[i - 2] = iter + 1;

                xs.Add(section[1]);

                int gap = CheckPoint(xs[0], h, funcNum);

                if (gap == 1)
                {
                    switch (fixMethod)
                    {
                        case 0:
                            xs[0] = FixByAverage(xs[0], eps, funcNum);
                            break;
                        case 1:
                            double[] res = FixByCut(xs[0], eps, funcNum);
                            xs[0] = res[1];
                            break;
                    }
                }
                else if (gap == 2)
                    return;

                borders[0] = xs[0];
                borders[1] = xs[xs.Count - 1];
                double sum = 0;
                double error = 0;
                for (int j = 0; j < xs.Count - 1; j++)
                {
                    if (isShift)
                    {
                        xs[j] = gapShift;
                        isShift = false;
                    }

                    int cur_gap = CheckPoint(xs[j + 1], h, funcNum);
                    if (cur_gap == 1)
                    {
                        switch (fixMethod)
                        {
                            case 0:
                                xs[j + 1] = FixByAverage(xs[j + 1], eps, funcNum);
                                break;
                            case 1:
                                double[] res = FixByCut(xs[0], eps, funcNum);
                                xs[j + 1] = res[0];
                                gapShift = res[1];
                                isShift = true;
                                break;
                        }
                    }
                    else if (cur_gap == 2)
                        return;

                    sum += (funcs[funcNum](xs[j]) + funcs[funcNum](xs[j + 1])) * h / 2;

                    error = Math.Max(error, CalcError(xs[j], h, funcNum));
                }
                error = Math.Max(error, CalcError(xs[i], h, funcNum));
                results[loopCount] = sum;
                errors[loopCount] = error;
                loopCount++;
            }

        }

        public static double?[] GetResults()
        {
            return results;
        }

        public static double?[] GetErrors()
        {
            return errors;
        }

        public static List<double> GetFGap()
        {
            return firstGaps;
        }

        public static double GetSecGap()
        {
            return secondGap;
        }

        public static int[] GetIters()
        {
            return iters;
        }

        public static double[] GetBorders()
        {
            return borders;
        }
    }
}

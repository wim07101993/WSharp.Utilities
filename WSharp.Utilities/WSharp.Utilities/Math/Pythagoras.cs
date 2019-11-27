using System;
using M = System.Math;

namespace WSharp.Utilities.Math
{
    public static class Pythagoras
    {
        public static double Solve(double a, double b) => M.Sqrt(a * a + b * b);

        public static double Solve(ref double? a, ref double? b, ref double? c)
        {
            if (a == null && b != null && c != null)
            {
                a = M.Sqrt(c.Value * c.Value - b.Value * b.Value);
                return a.Value;
            }
            else if (a != null && b == null && c!= null)
            {
                b = M.Sqrt(c.Value * c.Value - a.Value * a.Value);
                return b.Value;
            }
            else if (a != null && b != null && c == null)
            {
                c = M.Sqrt(a.Value * a.Value + b.Value * b.Value);
                return c.Value;
            }

            int nullCount = 0;
            if (a == null)
                nullCount++;
            if (b == null)
                nullCount++;
            if (c == null)
                nullCount++;

            if (nullCount < 1)
                throw new InvalidOperationException("Cannot calculate Pythagoras theorem with only one argument");
            throw new InvalidOperationException("Cannot calculate Pythagoras theorem when all variables are known");
        }

    }
}

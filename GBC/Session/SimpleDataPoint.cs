using System;

namespace GBC
{
    public class SimpleDataPoint
    {
        public SimpleDataPoint(double n1, double p, double n2)
        {
            N1 = n1;
            N2 = n2;
            P = p;
        }

        public double N1 { get; }
        public double N2 { get; }
        public double N => (N1 + N2) / 2;
        public double P { get; }
        public double Diff => P - N;

        public bool IsOutlier(double threshold) => Math.Abs(N2 - N1) > threshold;

    }
}

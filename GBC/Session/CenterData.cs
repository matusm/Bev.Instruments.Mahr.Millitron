using System;

namespace GBC
{
    public class CenterData
    {
        public CenterData(double n1, double p1, double p2, double n2)
        {
            N1 = n1;
            P1 = p1;
            P2 = p2;
            N2 = n2;
        }

        public CenterData() : this(double.NaN, double.NaN, double.NaN, double.NaN) { }

        public double N1 { get; }
        public double P1 { get; }
        public double P2 { get; }
        public double N2 { get; }

        public double MeanN => (N1 + N2) / 2;
        public double MeanP => (P1 + P2) / 2;
        public double DiffCenter => MeanP - MeanN;

        public bool IsOutlier(double threshold)
        {
            double d1 = P1 - N1;
            double d2 = P2 - N1;
            return (Math.Abs(d1 - d2) > threshold);
        }
    }
}

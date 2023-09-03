using System;

namespace GBC
{
    public class VariationData
    {
        public VariationData(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public double A { get; }
        public double B { get; }
        public double C { get; }
        public double D { get; }

        public double Fu => Math.Abs(Math.Min(Math.Min(Math.Min(A, B), Math.Min(C, D)), 0));
        public double Fo => Math.Max(Math.Max(Math.Max(A, B), Math.Max(C, D)), 0);
        public double V => Fu + Fo;

    }
}

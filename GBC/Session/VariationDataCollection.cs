using System.Collections.Generic;
using System.Linq;
using At.Matus.StatisticPod;

namespace GBC
{
    public class VariationDataCollection
    {
        private readonly List<VariationData> variations = new List<VariationData>();
        private readonly StatisticPod spA = new StatisticPod();
        private readonly StatisticPod spB = new StatisticPod();
        private readonly StatisticPod spC = new StatisticPod();
        private readonly StatisticPod spD = new StatisticPod();
        private readonly StatisticPod spV = new StatisticPod();
        private readonly StatisticPod spFu = new StatisticPod();
        private readonly StatisticPod spFo = new StatisticPod();

        public int NumberOfSamples => variations.Count();
        public VariationData[] Samples => variations.ToArray();
        public VariationData AverageVariation => new VariationData(AverageA, AverageB, AverageC, AverageD);

        public double AverageA => spA.AverageValue;
        public double AverageB => spB.AverageValue;
        public double AverageC => spC.AverageValue;
        public double AverageD => spD.AverageValue;
        public double AverageV => spV.AverageValue;
        public double AverageFu => spFu.AverageValue;
        public double AverageFo => spFo.AverageValue;

        public double RangeA => spA.Range;
        public double RangeB => spB.Range;
        public double RangeC => spC.Range;
        public double RangeD => spD.Range;
        public double RangeV => spV.Range;
        public double RangeFu => spFu.Range;
        public double RangeFo => spFo.Range;

        public double StandardDeviationA => spA.StandardDeviation;
        public double StandardDeviationB => spB.StandardDeviation;
        public double StandardDeviationC => spC.StandardDeviation;
        public double StandardDeviationD => spD.StandardDeviation;
        public double StandardDeviationV => spV.StandardDeviation;
        public double StandardDeviationFu => spFu.StandardDeviation;
        public double StandardDeviationFo => spFo.StandardDeviation;

        public void Add(VariationData dataPoint)
        {
            variations.Add(dataPoint);
            spA.Update(dataPoint.A);
            spB.Update(dataPoint.B);
            spC.Update(dataPoint.C);
            spD.Update(dataPoint.D);
            spV.Update(dataPoint.V);
            spFu.Update(dataPoint.Fu);
            spFo.Update(dataPoint.Fo);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using At.Matus.StatisticPod;

namespace GBC
{
    public class CenterDataCollection
    {
        private List<CenterData> dataPoints = new List<CenterData>();
        private List<double> diff = new List<double>();
        private List<double> drift = new List<double>();
        private StatisticPod spDiff = new StatisticPod();

        public int NumberOfSamples => dataPoints.Count();
        public CenterData[] Samples => dataPoints.ToArray();
        public double AverageDiff => spDiff.AverageValue;
        public double RangeDiff => spDiff.Range;
        public double StandardDeviationDevDiff => spDiff.StandardDeviation;
        public double MaxDrift => drift.Last();

        public void Add(CenterData dataPoint)
        {
            dataPoints.Add(dataPoint);
            diff.Add(dataPoint.DiffCenter);
            spDiff.Update(dataPoint.DiffCenter);
            drift.Add(dataPoint.MeanN - dataPoints.First().MeanN);
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Bev.UI;

namespace Bev.Instruments.Mahr.Millitron
{
    public class Comparator
    {
        private readonly Millitron1240 millitron;
        private readonly IProbeMover probeMover;
        private const int _pollPeriod = 200;        // time between consecutive readings, in ms
        private const int D_PROBEUP = 1100;         // minimum delay time to assure that probe is completely up, in ms

        public Comparator(Millitron1240 millitron, IProbeMover probeMover)
        {
            this.millitron = millitron;
            this.probeMover = probeMover;
        }

        public Comparator(Millitron1240 millitron) : this(millitron, new NullProbeMover()) { }

        public double MakeMeasurement(string pointName, int holdTime)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            AudioUI.Beep();
            Console.Write($"\n     {pointName}:");   // tell user which gauge block (or probing point) should be used
            //Console.Write($"\n\a     {pointName}:");   // tell user which gauge block (or probing point) should be used
            if (holdTime > 0) // use automatic probe movement
            {
                LiftProbeFor(holdTime);
            }
            Wait4Lift();        // wait for a lift of the probes (in auto mode probe is still lifted)
            double reading = GetValidValue();    // wait for the first valid value
            DateTime startOfDrop = DateTime.UtcNow;
            do
            {
                reading = GetValidValue();
                Console.Write($"\r     {pointName}: {reading,10:+0;-0} nm");
            } while ((DateTime.UtcNow - startOfDrop).TotalSeconds < millitron.SettlingTime);
            return reading;
        }

        public double MakeMeasurement(string message) => MakeMeasurement(message, 0);

        private void DropProbe() => probeMover.DropProbe();

        private void LiftProbe() => probeMover.LiftProbe();

        private void LiftProbeFor(int holdTime)
        {
            LiftProbe();
            Thread.Sleep(Math.Max(holdTime, D_PROBEUP));
            DropProbe();
        }

        // poll and waiz for the first valid value 
        private double GetValidValue()
        {
            string answ;
            do
            {
                Thread.Sleep(Math.Abs(_pollPeriod - millitron.QueryDuration));
                answ = millitron.Query("M");
            } while (answ.Contains('E'));
            answ = answ.Remove(0, Math.Min(3, answ.Length));
            double d = millitron.ParseDoubleFrom(answ);
            return millitron.ConvertRawToNm(d);
        }

        // Returns on the first error (usually an overflow) while polling measurement values.
        private void Wait4Lift()
        {
            string answ;
            do
            {
                Thread.Sleep(Math.Abs(_pollPeriod - millitron.QueryDuration));
                answ = millitron.Query("M");
            } while (!answ.Contains('E'));
        }

    }
}

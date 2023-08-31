using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bev.Instruments.Mahr.Millitron;
using Bev.UI;

namespace GBC
{
    class Program
    {
        static void Main(string[] args)
        {
            string mahrPort = "COM1";
            string conradPort = "COM12";
            string vaisalaPort = "10.10.10.98";

            Console.Clear();
            ConsoleUI.Welcome();
            ConsoleUI.WriteLine();

            #region Instantiate all hardware objects
            Millitron1240 millitron;
            IProbeMover probeMover;
            Environment environment;
            Comparator comparator;

            using (new InfoOperation("initializing comparator"))
            {
                millitron = new Millitron1240(mahrPort);
            }

            using (new InfoOperation("initializing probe mover vor comparator"))
            {
                probeMover = new NullProbeMover(); // here we must chose the correct one
                comparator = new Comparator(millitron, probeMover);
            }

            using(new InfoOperation("initializing thermo-hygrometer"))
            {
                IThermoHygrometer thTransmitter = new VaisalaHmtThermometer(vaisalaPort);
                environment = new Environment(thTransmitter);
            }
            #endregion






            // test cases
            var x = comparator.MakeMeasurement("X");
            var y = comparator.MakeMeasurement("Y");
            Console.WriteLine();
            Console.WriteLine($"X-Y: {x-y:F1} nm");
            millitron.Reset();

        }
    }
}

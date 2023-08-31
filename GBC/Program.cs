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

            // instantiate all hardware objects
            Millitron1240 millitron;
            IProbeMover probeMover;
            Environment environment;
            Comparator comparator;

            using (new InfoOperation("initializing comparator"))
            {
                millitron = new Millitron1240(mahrPort);
            }

            using(new InfoOperation("initializing thermo-hygrometer"))
            {
                IThermoHygrometer thTransmitter = new VaisalaHmtThermometer(vaisalaPort);
                environment = new Environment(thTransmitter);
            }
            
            probeMover = new NullProbeMover();
            // instantiate high level objects 
            comparator = new Comparator(millitron, probeMover);






            // test cases
            var x = comparator.MakeMeasurement("X");
            var y = comparator.MakeMeasurement("Y");
            Console.WriteLine();
            Console.WriteLine($"X-Y: {x-y:F1} nm");
            millitron.Reset();



        }
    }
}

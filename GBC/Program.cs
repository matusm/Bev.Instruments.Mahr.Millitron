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

            // instantiate all hardware objects
            Millitron1240 millitron = new Millitron1240(mahrPort);
            IProbeMover probeMover = new NullProbeMover();
            IThermoHygrometer thTransmitter = new VaisalaHmtThermometer(vaisalaPort);
            // instantiate high level objects 
            Comparator comparator = new Comparator(millitron, probeMover);
            Environment environment = new Environment(thTransmitter);






            // test cases
            var x = comparator.MakeMeasurement("X");
            var y = comparator.MakeMeasurement("Y");
            Console.WriteLine();
            Console.WriteLine($"X-Y: {x-y:F1} nm");
            millitron.Reset();



        }
    }
}

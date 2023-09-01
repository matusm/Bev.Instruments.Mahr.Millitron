using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Bev.Instruments.Mahr.Millitron;
using Bev.UI;
using GBC.Properties;

namespace GBC
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = new Settings();

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
                millitron = new Millitron1240(settings.ComPortMillitron);
            }

            using (new InfoOperation("initializing probe mover for comparator"))
            {
                probeMover = new NullProbeMover(); // here we must chose the correct one
                //probeMover = new ConradProbeMover(settings.ComPortConrad, settings.ChannelConrad);
                comparator = new Comparator(millitron, probeMover);
            }

            using(new InfoOperation("initializing thermo-hygrometer"))
            {
                IThermoHygrometer thTransmitter = new VaisalaHmtThermometer(settings.IPEnvironment);
                environment = new Environment(thTransmitter);
            }
            #endregion

            DiagnosticOutput();
            UserParameters userParameters = new UserParameters();
            userParameters.QueryUserInput();


            // test cases
            var x = comparator.MakeMeasurement("X");
            var y = comparator.MakeMeasurement("Y");
            Console.WriteLine();
            Console.WriteLine($"X-Y: {x-y:F1} nm");
            millitron.Reset();


            /********************************************************/


            void DiagnosticOutput()
            {
                ConsoleUI.WriteLine();
                ConsoleUI.WriteLine("Komparator");
                ConsoleUI.WriteLine(string.Format("  {0}", millitron.InstrumentID));
                ConsoleUI.WriteLine(string.Format("  Wartezeit: {0} s", millitron.SettlingTime));
                //ConsoleUI.WriteLine(string.Format("  Tasterhubzeit: {0} s", (double)liftDelay / 1000.0));
                ConsoleUI.WriteLine(string.Format("  Faktor A: {0,7:0.0000}", millitron.CorrectionProbeA));
                ConsoleUI.WriteLine(string.Format("  Faktor B: {0,7:0.0000}", millitron.CorrectionProbeB));
                ConsoleUI.WriteLine(string.Format("  Auflösungserhöhung: {0}", millitron.ResolutionEnhancement));
                ConsoleUI.WriteLine(string.Format("  Messwertintegrationszeit: {0:0.0000} s", millitron.IntegrationTime));
                ConsoleUI.WriteLine("Thermometer");
                ConsoleUI.WriteLine("  " + environment.TransmitterID);
                ConsoleUI.WriteLine("Kalibrierumfang");
                //if (ConsoleUI.FlagCenter)
                //    ConsoleUI.WriteLine(string.Format("  Mittenmassmessung ({0} x)", ConsoleUI.NumRep));
                //if (ConsoleUI.Flag5Point)
                //    ConsoleUI.WriteLine(string.Format("  Abweichungsspanne ({0} x)", ConsoleUI.NumRep5));
                ConsoleUI.WriteLine();
            }

        }
    }
}

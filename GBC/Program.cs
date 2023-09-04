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
            MyCommandLine options = new MyCommandLine(args);
            GaugeBlock preuflingGB;
            GaugeBlock normalGB;

            Console.Clear();
            ConsoleUI.Welcome();
            ConsoleUI.WriteLine();

            #region Instantiate all hardware objects
            Millitron1240 millitron;
            IProbeMover probeMover;
            Environmental environment;
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
                environment = new Environmental(thTransmitter);
            }
            #endregion

            DiagnosticOutput();


            SessionData sessionData = new SessionData();
            sessionData.QuerySessionData();
            preuflingGB = sessionData.QueryTestBlock();
            normalGB = sessionData.QueryStandardBlock();

            #region Center length measurement loop
            CenterDataCollection centerDataCollection = new CenterDataCollection();
            int numOutlierCenter = 0;
            if (true) // TODO from command line option
            {
                bool outlierDetected;
                environment.Update();
                
                outlierDetected = false;
                CenterData dataPoint = new CenterData(); // just to suppress compiler errors
                for (int i = 0; i <settings.Loops; i++)
                {
                    Console.Clear();
                    Console.WriteLine("Bestimmung des Mittenmasses");
                    if (i > 0) Console.WriteLine($"(letzter Wert: {dataPoint.DiffCenter} nm)\n");
                    if (outlierDetected) Console.Write("! Wiederholung ! ");
                    Console.WriteLine($"{i+1}. von {settings.Loops} Messungen");

                    double n1 = comparator.MakeMeasurement("N", settings.LiftDelay);
                    double p1 = comparator.MakeMeasurement("P", settings.LiftDelay);
                    double p2 = comparator.MakeMeasurement("P", settings.LiftDelay);
                    double n2 = comparator.MakeMeasurement("N", settings.LiftDelay);

                    dataPoint = new CenterData(n1, p1, p2, n2);
                    outlierDetected = dataPoint.IsOutlier(settings.OutlierThreshold);
                    environment.Update();

                    if (outlierDetected)
                    {
                        numOutlierCenter++;
                        i--;
                    }
                    else
                    {
                        centerDataCollection.Add(dataPoint);
                    }
                }
                // it is necessary to set the temperatures in advance
                normalGB.Temperature = environment.Temperature;
                preuflingGB.Temperature = environment.Temperature;
                preuflingGB.CalibrateWith(normalGB, centerDataCollection.AverageDiff/1000);

            }
            #endregion

            #region 5-point measurement loop
            VariationDataCollection variationDataCollection = new VariationDataCollection();
            int numOutlier5Point = 0;
            if (true)
            {
                environment.Update();
                for (int i = 0; i < settings.Loops5Point; i++)
                {
                    Console.Clear();
                    Console.WriteLine("Bestimmung der Abweichungsspanne (Normal kann entfernt werden)");
                    Console.WriteLine($"{i + 1}. von {settings.Loops5Point} Messungen");
                    Console.WriteLine(GbTextGraph(5));
                    // Antastsequenz: M C M A M B M D M
                    double m = comparator.MakeMeasurement("M", settings.LiftDelay);
                    SimpleDataPoint dataPointC = MeasureCorner("C", m);
                    SimpleDataPoint dataPointA = MeasureCorner("A", dataPointC.N2);
                    SimpleDataPoint dataPointB = MeasureCorner("B", dataPointA.N2);
                    SimpleDataPoint dataPointD = MeasureCorner("D", dataPointB.N2);

                    var variationData = new VariationData(dataPointA.Diff, dataPointB.Diff, dataPointC.Diff, dataPointD.Diff);
                    variationDataCollection.Add(variationData);
                }
            }
            #endregion


            millitron.Reset();


            /******************************************************************************/
            #region Local functions
            SimpleDataPoint MeasureCorner(string cornerName, double previousCenterReading)
            {
                bool outlierDetected;
                double m1, m2, p;
                m2 = previousCenterReading;
                SimpleDataPoint cornerValue;
                do
                {
                    m1 = m2;
                    p = comparator.MakeMeasurement(cornerName, settings.LiftDelay);
                    m2 = comparator.MakeMeasurement("M", settings.LiftDelay);
                    cornerValue = new SimpleDataPoint(m1, p, m2);
                    outlierDetected = cornerValue.IsOutlier(settings.OutlierThreshold5Point);
                    if (outlierDetected)
                    { 
                        Console.WriteLine("! Wiederholung! ");
                        numOutlier5Point++;
                    }
                } while (outlierDetected);
                return cornerValue;
            }

            /******************************************************************************/

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

            /******************************************************************************/

            string GbTextGraph(int leadingSpaces)
            {
                string sSpaces = new string(' ', leadingSpaces);
                string bild = "\n";
                bild += sSpaces + "┌───────────────────┐\n";
                bild += sSpaces + "│ A               B │\n";
                bild += sSpaces + "│         M         │\n";
                bild += sSpaces + "│ C               D │\n";
                bild += sSpaces + "└───────────────────┘\n";
                return bild;
            }

            #endregion
            /******************************************************************************/

        }
    }
}

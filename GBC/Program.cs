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
                if(options.AutoMoveProbe)
                {
                    probeMover = new ConradProbeMover(settings.ComPortConrad, settings.ChannelConrad);
                }
                else
                {
                    probeMover = new NullProbeMover();
                }
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
            normalGB = new GaugeBlock();
            if(options.PerformCenter) normalGB = sessionData.QueryStandardBlock();

            #region Center length measurement loop
            CenterDataCollection centerDataCollection = new CenterDataCollection();
            int numOutlierCenter = 0;
            if (options.PerformCenter)
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
            if (options.PerformVariation)
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
                    environment.Update();
                }
                preuflingGB.AddVariationData(variationDataCollection.AverageVariation);
                preuflingGB.Temperature = environment.Temperature;
            }
            #endregion

            // generic test
            Console.WriteLine(preuflingGB);


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
                        AudioUI.BeepLow();
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
                ConsoleUI.WriteLine($"  {millitron.InstrumentID}");
                ConsoleUI.WriteLine($"  Wartezeit: {millitron.SettlingTime} s");
                //ConsoleUI.WriteLine(string.Format("  Tasterhubzeit: {0} s", (double)liftDelay / 1000.0));
                ConsoleUI.WriteLine($"  Faktor A: {millitron.CorrectionProbeA,7:0.0000}");
                ConsoleUI.WriteLine($"  Faktor B: {millitron.CorrectionProbeB,7:0.0000}");
                ConsoleUI.WriteLine($"  Auflösungserhöhung: {millitron.ResolutionEnhancement}");
                ConsoleUI.WriteLine($"  Messwertintegrationszeit: {millitron.IntegrationTime:0.0000} s");
                ConsoleUI.WriteLine("Thermo-Hygrometer");
                ConsoleUI.WriteLine($"  {environment.TransmitterID}");
                ConsoleUI.WriteLine("Kalibrierumfang");
                if (options.PerformCenter)
                    ConsoleUI.WriteLine(string.Format("  Mittenmassmessung ({0} x)", settings.Loops));
                if (options.PerformVariation)
                    ConsoleUI.WriteLine(string.Format("  Abweichungsspanne ({0} x)", settings.Loops5Point));
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

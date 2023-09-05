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
            DateTime sessionStart;
            DateTime sessionStop;
            string reportFilename;

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

            DisplayStartInfo();

            SessionData sessionData = new SessionData();
            sessionData.QuerySessionData();
            GaugeBlock preuflingGB = sessionData.QueryTestBlock();
            GaugeBlock normalGB = new GaugeBlock();
            if(options.PerformCenter) 
                normalGB = sessionData.QueryStandardBlock();

            sessionStart = DateTime.UtcNow;
            reportFilename = $"{settings.LogDirectory}GBC_{sessionStart.ToString("yyyyMMdd-HHmm")}.txt";

            #region Center length measurement loop
            CenterDataCollection centerDataCollection = new CenterDataCollection();
            int numOutlierCenter = 0;
            if (options.PerformCenter)
            {
                bool outlierDetected = false;
                environment.Update();
                CenterData dataPoint = new CenterData(); // just to suppress compiler errors
                for (int i = 0; i <settings.Loops; i++)
                {
                    Console.Clear();
                    Console.WriteLine("Bestimmung des Mittenmasses");
                    if (i > 0) Console.WriteLine($"(letzter Wert: {dataPoint.DiffCenter:F0} nm)\n");
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
                if(!options.PerformCenter) // only if no center length measurement
                    preuflingGB.Temperature = environment.Temperature;
            }
            #endregion

            sessionStop = DateTime.UtcNow;

            string reportPage = GenerateReport();
            reportPage.ToConsole();
            reportPage.ToFile(reportFilename);

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
                        Console.Write(" !Wiederholung! ");
                        numOutlier5Point++;
                    }
                } while (outlierDetected);
                return cornerValue;
            }

            /******************************************************************************/

            void DisplayStartInfo()
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

            /******************************************************************************/

            string GenerateReport()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(GenerateReportPart0());
                sb.Append(GenerateReportPart1());
                sb.Append(GenerateReportPart2());
                sb.Append(GenerateReportPart3());
                sb.Append(GenerateReportPart4());
                return sb.ToString().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            }

            /******************************************************************************/

            string GenerateReportPart0()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine($" > program {ConsoleUI.Title}, version {ConsoleUI.Version}");
                sb.AppendLine($" > length instrument: {millitron.InstrumentID}");
                sb.AppendLine($" > environment: {environment.TransmitterID}");
                sb.AppendLine($" > filename: {reportFilename}");
                sb.AppendLine();
                sb.AppendLine($"   Auftrag:        {sessionData.Auftrag}");
                sb.AppendLine($"   T-Zahl:         {sessionData.TZahl}");
                sb.AppendLine($"   Kommentar:      {sessionData.Kommentar}");
                sb.AppendLine($"   Beobachter:     {sessionData.Beobachter}");
                sb.AppendLine($"   Datum:          {sessionStart.ToString("dd-MM-yyyy HH:mm")}");
                sb.AppendLine($"   Kalibrierdauer: {(sessionStop-sessionStart).TotalMinutes:F0} min");
                sb.AppendLine($"   Lufttemperatur: {environment.Temperature:0.00} °C ± {environment.TemperatureScatter:0.00} °C");
                sb.AppendLine($"   Temp.-Drift:    {environment.TemperatureDrift:+0.00;-0.00} °C");
                sb.AppendLine($"   Luftfeuchte:    {environment.Humidity:0.} % ± {environment.HumidityScatter:0.} %");
                sb.AppendLine();


                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart1()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "   -----------------------------------------------------------------------");
                sb.AppendLine( "   EINGABEWERTE:");
                sb.AppendLine($"     Pruefling:             {preuflingGB.Designation} ({preuflingGB.Manufacturer})");
                if (options.PerformCenter) sb.AppendLine($"     Normal:                {normalGB.Designation} ({normalGB.Manufacturer})");
                sb.AppendLine($"     Nennlänge:             {preuflingGB.NominalLength} mm");
                if (options.PerformCenter) sb.AppendLine($"     Abweichung, Normal:    {normalGB.CenterDeviation:+0.000;-0.000} µm");
                if (options.PerformCenter) sb.AppendLine($"     Abplattungskorrektur:  {preuflingGB.ElasticCorrection:+0.000;-0.000} µm");
                sb.AppendLine($"     Material, Prüfling:    {preuflingGB.MaterialBezeichnung}");
                if (options.PerformCenter) sb.AppendLine($"     Material, Normal:      {normalGB.MaterialBezeichnung}");
                sb.AppendLine($"     Temperatur, Prüfling:  {preuflingGB.Temperature,6:0.000} °C");
                if (options.PerformCenter) sb.AppendLine($"     Temperatur, Normal:    {normalGB.Temperature,6:0.000} °C");
                sb.AppendLine($"     alpha, Prüfling:       {preuflingGB.Material.Alpha,4:0.0} ppm/K");
                if (options.PerformCenter) sb.AppendLine($"     alpha, Normal:         {normalGB.Material.Alpha,4:0.0} ppm/K");
                if (options.PerformCenter) sb.AppendLine($"     Temperaturkorrektur:   {preuflingGB.TemperatureCorrection:+0.000;-0.000} µm (errechnet)");
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart2()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   PARAMETER:");

                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart3()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   MESSWERTE (alle Angaben in nm):");
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart4()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   ERGEBNIS:\n");
                return sb.ToString();
            }

            /******************************************************************************/




            /******************************************************************************/
            #endregion
            /******************************************************************************/

        }
    }
}

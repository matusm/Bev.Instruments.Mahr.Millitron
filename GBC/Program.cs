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

            // user just wants to reset the Millitron
            if(options.ResetMillitron)
            {
                using (new InfoOperation("Reset comparator"))
                {
                    Millitron1240 device = new Millitron1240(settings.ComPortMillitron);
                    Thread.Sleep(100);
                    device.Reset();
                    Thread.Sleep(2000);
                    ConsoleUI.WriteLine();
                }
                ConsoleUI.WaitForKey("Zum Beenden eine Taste drücken...");
                return;
            }

            #region Instantiate all hardware objects
            Millitron1240 millitron;
            IProbeMover probeMover;
            Environmental environmental;
            Comparator comparator;

            using (new InfoOperation("Initializing comparator"))
            {
                millitron = new Millitron1240(settings.ComPortMillitron);
            }

            using (new InfoOperation("Initializing probe mover for comparator"))
            {
                if (options.AutoMoveProbe)
                {
                    probeMover = new ConradProbeMover(settings.ComPortConrad, settings.ChannelConrad);
                }
                else
                {
                    probeMover = new NullProbeMover();
                }
                comparator = new Comparator(millitron, probeMover);
            }

            using (new InfoOperation("Initializing thermo-hygrometer"))
            {
                IThermoHygrometer thTransmitter = new VaisalaHmtThermometer(settings.IPEnvironment);
                environmental = new Environmental(thTransmitter);
            }
            #endregion

            DisplayStartInfo();

            SessionData sessionData = new SessionData();
            sessionData.QuerySessionData();
            GaugeBlock preuflingGB = sessionData.QueryTestBlock();
            GaugeBlock normalGB = new GaugeBlock();
            if (options.PerformCenter)
                normalGB = sessionData.QueryStandardBlock();

            sessionStart = DateTime.UtcNow;
            reportFilename = $"{settings.LogDirectory}GBC_{sessionStart.ToString("yyyyMMdd-HHmm")}.txt";

            #region Center length measurement loop
            CenterDataCollection centerDataCollection = new CenterDataCollection();
            int numOutlierCenter = 0;
            if (options.PerformCenter)
            {
                bool outlierDetected = false;
                environmental.Update();
                CenterData dataPoint = new CenterData(); // just to suppress compiler errors
                for (int i = 0; i < settings.Loops; i++)
                {
                    Console.Clear();
                    Console.WriteLine("Bestimmung des Mittenmasses");
                    if (i > 0) Console.WriteLine($"(letzter Wert: {dataPoint.DiffCenter:F0} nm)\n");
                    if (outlierDetected) Console.Write("! Wiederholung ! ");
                    Console.WriteLine($"{i + 1}. von {settings.Loops} Messungen");

                    double n1 = comparator.MakeMeasurement("N", settings.LiftDelay);
                    double p1 = comparator.MakeMeasurement("P", settings.LiftDelay);
                    double p2 = comparator.MakeMeasurement("P", settings.LiftDelay);
                    double n2 = comparator.MakeMeasurement("N", settings.LiftDelay);

                    dataPoint = new CenterData(n1, p1, p2, n2);
                    outlierDetected = dataPoint.IsOutlier(settings.OutlierThreshold);
                    environmental.Update();

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
                normalGB.Temperature = environmental.Temperature;
                preuflingGB.Temperature = environmental.Temperature;
                preuflingGB.CalibrateWith(normalGB, centerDataCollection.AverageDiff / 1000);
            }
            #endregion

            #region 5-point measurement loop
            VariationDataCollection variationDataCollection = new VariationDataCollection();
            int numOutlier5Point = 0;
            if (options.PerformVariation)
            {
                environmental.Update();
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
                    environmental.Update();
                }
                preuflingGB.AddVariationData(variationDataCollection.AverageVariation);
                if (!options.PerformCenter) // only if no center length measurement
                    preuflingGB.Temperature = environmental.Temperature;
            }
            #endregion

            sessionStop = DateTime.UtcNow;

            string reportPage = GenerateReport();
            reportPage.ToConsole();
            reportPage.ToFile(reportFilename);

            millitron.Reset();

            ConsoleUI.WaitForKey("Zum Beenden eine Taste drücken...");

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
                        Thread.Sleep(400);
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
                ConsoleUI.WriteLine($"  {environmental.TransmitterID}");
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
                sb.AppendLine($" > Program {ConsoleUI.Title}, version {ConsoleUI.FullVersion}");
                sb.AppendLine($" > Comparator: {millitron.InstrumentID}");
                sb.AppendLine($" > Environment: {environmental.TransmitterID}");
                sb.AppendLine($" > Filename: {reportFilename}");
                sb.AppendLine();
                sb.AppendLine($"   Auftrag:        {sessionData.Auftrag}");
                sb.AppendLine($"   T-Zahl:         {sessionData.TZahl}");
                sb.AppendLine($"   Kommentar:      {sessionData.Kommentar}");
                sb.AppendLine($"   Beobachter:     {sessionData.Beobachter}");
                sb.AppendLine($"   Datum:          {sessionStart.ToString("dd-MM-yyyy HH:mm")} (UTC)");
                sb.AppendLine($"   Kalibrierdauer: {(sessionStop - sessionStart).TotalMinutes:F0} min");
                sb.AppendLine($"   Lufttemperatur: {environmental.Temperature:0.00} °C ± {environmental.TemperatureScatter:0.00} °C");
                sb.AppendLine($"   Temp.-Drift:    {environmental.TemperatureDrift:+0.00;-0.00} °C");
                sb.AppendLine($"   Luftfeuchte:    {environmental.Humidity:0.} %");
                sb.AppendLine();
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart1()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   EINGABEWERTE:");
                sb.AppendLine($"     Pruefling:             {preuflingGB.Designation} ({preuflingGB.Manufacturer})");
                if (options.PerformCenter) sb.AppendLine($"     Normal:                {normalGB.Designation} ({normalGB.Manufacturer})");
                sb.AppendLine($"     Nennlänge:             {preuflingGB.NominalLength} mm");
                if (options.PerformCenter) sb.AppendLine($"     Abweichung, Normal:    {normalGB.CenterDeviation:+0.000;-0.000} µm");
                sb.AppendLine($"     Material, Prüfling:    {preuflingGB.MaterialBezeichnung}");
                if (options.PerformCenter) sb.AppendLine($"     Material, Normal:      {normalGB.MaterialBezeichnung}");
                sb.AppendLine($"     Temperatur, Prüfling:  {preuflingGB.Temperature,6:0.000} °C");
                if (options.PerformCenter) sb.AppendLine($"     Temperatur, Normal:    {normalGB.Temperature,6:0.000} °C");
                sb.AppendLine($"     alpha, Prüfling:       {preuflingGB.Material.Alpha,4:0.0} ppm/K");
                if (options.PerformCenter) sb.AppendLine($"     alpha, Normal:         {normalGB.Material.Alpha,4:0.0} ppm/K");
                if (options.PerformCenter) sb.AppendLine($"     Temperaturkorrektur:   {preuflingGB.TemperatureCorrection:+0.000;-0.000} µm (errechnet)");
                if (options.PerformCenter) sb.AppendLine($"     Abplattungskorrektur:  {preuflingGB.ElasticCorrection:+0.000;-0.000} µm");
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart2()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   PARAMETER:");
                sb.AppendLine($"     Messzyklen:     {settings.Loops} (Mittenmaß)");
                sb.AppendLine($"     Messzyklen:     {settings.Loops5Point} (Abweichungsspanne)");
                sb.AppendLine($"     Wartezeit:      {settings.SettlingTime} s");
                sb.AppendLine($"     Tasterhubzeit:  {settings.LiftDelay / 1000.0:F1} s");
                sb.AppendLine($"     Grenzwert 1:    {settings.OutlierThreshold} nm (Mittenmaß)");
                sb.AppendLine($"     Grenzwert 2:    {settings.OutlierThreshold5Point} nm (Abweichungsspanne)");
                sb.AppendLine($"     Korrekturfaktor Taster A (oben):  {millitron.CorrectionProbeA:0.0000} * {millitron.ResolutionEnhancement}");
                sb.AppendLine($"     Korrekturfaktor Taster B (oben):  {millitron.CorrectionProbeB:0.0000} * {millitron.ResolutionEnhancement}");
                sb.AppendLine($"     Messwertintegrationszeit: {millitron.IntegrationTime:0.0000} s");
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart3()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   MESSWERTE (alle Angaben in nm):");
                if (options.PerformCenter)
                {
                    sb.AppendLine("     Mittenmaß:");
                    sb.AppendLine("      #        N          P          P          N      Drift        P-N");
                    for (int i = 0; i < centerDataCollection.NumberOfSamples; i++)
                    {
                        CenterData cd = centerDataCollection.Samples[i];
                        double drift = centerDataCollection.Drift[i];
                        sb.AppendLine($"     {i + 1,2}  {cd.N1,7:+0.;-0.}    {cd.P1,7:+0.;-0.}    {cd.P2,7:+0.;-0.}    {cd.N2,7:+0.;-0.}    {drift,7:+0.;-0.}    {cd.DiffCenter,7:+0.;-0.}");
                    }
                    sb.AppendLine();
                    sb.AppendLine($"     Mittel(P-N) = {centerDataCollection.AverageDiff:+0.0;-0.0} nm    sigma = {centerDataCollection.StandardDeviationDiff:0.0} nm    Spanne = {centerDataCollection.RangeDiff:0.0} nm");
                    if (numOutlierCenter == 1) sb.AppendLine("     1 Wiederholmessung");
                    if (numOutlierCenter > 1) sb.AppendLine($"     {numOutlierCenter} Wiederholmessungen");
                }
                if (options.PerformVariation)
                {
                    if(options.PerformCenter) sb.AppendLine();
                    sb.AppendLine("     5-Punkt-Messung:");
                    sb.AppendLine($"     A-M ={variationDataCollection.AverageA,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationA,3:0.} nm     Spanne ={variationDataCollection.RangeA,3:0.} nm");
                    sb.AppendLine($"     B-M ={variationDataCollection.AverageB,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationB,3:0.} nm     Spanne ={variationDataCollection.RangeB,3:0.} nm");
                    sb.AppendLine($"     C-M ={variationDataCollection.AverageC,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationC,3:0.} nm     Spanne ={variationDataCollection.RangeC,3:0.} nm");
                    sb.AppendLine($"     D-M ={variationDataCollection.AverageD,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationD,3:0.} nm     Spanne ={variationDataCollection.RangeD,3:0.} nm");
                    sb.AppendLine("     ---------------------------------------------------");
                    sb.AppendLine($"     v   ={variationDataCollection.AverageV,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationV,3:0.} nm     Spanne ={variationDataCollection.RangeV,3:0.} nm");
                    sb.AppendLine($"     f_u ={variationDataCollection.AverageFu,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationFu,3:0.} nm     Spanne ={variationDataCollection.RangeFu,3:0.} nm");
                    sb.AppendLine($"     f_o ={variationDataCollection.AverageFo,6:+0.;-0.} nm     sigma ={variationDataCollection.StandardDeviationFo,3:0.} nm     Spanne ={variationDataCollection.RangeFo,3:0.} nm");
                    if (numOutlier5Point == 1) sb.AppendLine("     1 Wiederholmessung");
                    if (numOutlier5Point > 1) sb.AppendLine($"     {numOutlier5Point} Wiederholmessungen");
                }
                return sb.ToString();
            }

            /******************************************************************************/

            string GenerateReportPart4()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("   -----------------------------------------------------------------------");
                sb.AppendLine("   ERGEBNIS:\n");
                if (options.PerformCenter) sb.AppendLine($"     Mittenmaßabweichung:  f_c   = {preuflingGB.CenterDeviation:+0.000;-0.000} µm  (Mittenmaß: {preuflingGB.NominalLength + preuflingGB.CenterDeviation / 1000.0:0.000000} mm)");
                if (options.PerformVariation) sb.AppendLine($"     Abweichungsspanne:    v     = {preuflingGB.V:0.000} µm");
                if (options.PerformVariation) sb.AppendLine($"     untere Abweichung:    f_u   = {preuflingGB.Fu:0.000} µm");
                if (options.PerformVariation) sb.AppendLine($"     obere Abweichung:     f_o   = {preuflingGB.Fo:0.000} µm");
                if (options.PerformVariation && options.PerformCenter) sb.AppendLine($"     maximale Abweichung:  f_max = {preuflingGB.Fmax:0.000} µm");
                if (options.PerformVariation) sb.AppendLine(GbTextGraph(5));
                return sb.ToString();
            }

            /******************************************************************************/




            /******************************************************************************/
            #endregion
            /******************************************************************************/

        }
    }
}

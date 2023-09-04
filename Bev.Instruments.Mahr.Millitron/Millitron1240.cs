using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Bev.Instruments.Mahr.Millitron
{
    public class Millitron1240
    {
        private SerialPort serialPort;
        private int settlingTime;                   // in s
        private double resolutionEnhancement = 10;  // Aufloesung-Erhoehung
        private double corrFactorA = double.NaN;
        private double corrFactorB = double.NaN;
        private const int _delayAfterWrite = 50;    // delay time after sending a command to the millitron, in ms
        private const int _delayForResponse = 100;  // delay time after sending a command and reading the response, in ms

        public Millitron1240(string port)
        {
            StartDate = DateTime.UtcNow;
            serialPort = new SerialPort(port, 9600, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                ReadTimeout = 3000,
                WriteTimeout = 3000,
                RtsEnable = true,
                DtrEnable = true
            };
            serialPort.Open();
            Initialize();
            settlingTime = 10;  // quick and dirty
        }

        public string DevicePort => serialPort.PortName;
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentType { get; private set; }
        public string InstrumentSerialNumber => "---"; // no documented way to obtain the serial number
        public string InstrumentFirmewareVersion { get; private set; }
        public string InstrumentID => $"{InstrumentManufacturer} {InstrumentType} {InstrumentFirmewareVersion} @ {DevicePort}";
        public DateTime StartDate { get; }
        public double IntegrationTime { get; private set; } // Messwertintegrationszeit (p71)
        public double CorrectionProbeA => corrFactorA;
        public double CorrectionProbeB => corrFactorB;
        public double ResolutionEnhancement => resolutionEnhancement;
        public int SettlingTime // in s !
        {
            get { return settlingTime; }
            set
            {
                settlingTime = value;
                if (value < 1) settlingTime = 1;
                if (value > 60) settlingTime = 60;
            }
        }
        internal int QueryDuration => (_delayAfterWrite + _delayForResponse);

        public string Query(string sCommand) => Query(sCommand, 0, _delayForResponse);

        public string Query(string sCommand, int leadingChars) => Query(sCommand, leadingChars, _delayForResponse);

        public void Reset() => Query("R"); // operation takes a long time!

        public void Zero() => Query("Z");

        public double GetValue()
        {
            string answ = Query("M", 3);
            double d = ParseDoubleFrom(answ);
            return ConvertRawToNm(d);
        }

        // must be called only once!
        private void EnhanceResolution(double factor)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            string command = $"P52,1,{corrFactorA * factor:0.0000},2,{corrFactorB * factor:0.0000}";
            Query(command);
        }

        private void Initialize()
        {
            string identText = Query("I", 2, 5 * _delayAfterWrite); // = Aktivieren der Schnittstellensoftware (p63)
            serialPort.DiscardInBuffer();
            string[] tokens;
            char[] charSeparators = new char[] { ',' };
            tokens = identText.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length >= 3)
            {
                InstrumentManufacturer = tokens[0];
                InstrumentType = tokens[1];
                InstrumentFirmewareVersion = tokens[2];
            }
            // get correction factors for probes
            corrFactorA = GetCorrectionValueA();
            corrFactorB = GetCorrectionValueB();
            EnhanceResolution(resolutionEnhancement);
            // Set the integration time to 0,1024 s !!
            Query("P92,.1024");
            // get internal integration time
            Query("P92", 4); // for unknown reasons this must be called twice!
            IntegrationTime = ParseDoubleFrom(Query("P92", 4));
            // tolerancing on
            Query("P21,-10.0,10.0");
            // if requested set to zero
            Thread.Sleep(2000); // don't know why
            serialPort.DiscardInBuffer();
        }

        private double GetCorrectionValueA() => ParseDoubleFrom(Query("P52,1", 6));

        private double GetCorrectionValueB() => ParseDoubleFrom(Query("P52,2", 6));

        private string Query(string command, int leadingChars, int delay)
        {
            Send1240(command);
            Thread.Sleep(delay);
            string answ = Read1240();
            // following line is for debuging only
            // if(answ.Contains('E')) Console.WriteLine(">>> " + command + " caused " + answ);
            if (leadingChars <= 0) 
                return answ;
            answ = answ.Remove(0, Math.Min(leadingChars, answ.Length));
            return answ;
        }

        private void Send1240(string command)
        {
            byte[] cmd = Encoding.ASCII.GetBytes(command + "\r");
            serialPort.Write(cmd, 0, cmd.Length);
        }

        private string Read1240()
        {
            byte[] buffer = new byte[serialPort.BytesToRead];
            try
            {
                serialPort.Read(buffer, 0, buffer.Length);
                string str = Encoding.UTF8.GetString(buffer);
                return RemoveNewline(str);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string RemoveNewline(string str) => str.Replace("\n", "").Replace("\r", "");

        internal double ParseDoubleFrom(string token) => double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) ? value : double.NaN;

        internal double ConvertRawToNm(double rawValue) => 1000.0 * rawValue / resolutionEnhancement;

    }
}

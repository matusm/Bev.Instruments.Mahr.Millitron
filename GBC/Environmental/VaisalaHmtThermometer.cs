using Bev.Transmitter;

namespace GBC
{
    public class VaisalaHmtThermometer : IThermoHygrometer
    {

        private readonly BaseBevTransmitter hmt;

        public VaisalaHmtThermometer(string port)
        {
            hmt = new BevVaisalaHmt(port);
        }

        public string InstrumentID => $"{hmt.InstrumentManufacturer} {hmt.InstrumentType} {hmt.InstrumentSerialNumber}";

        public double GetHumidity() => EditInvalidValues(hmt.RelHumidity);

        public double GetTemperature() => EditInvalidValues(hmt.AirTemperature);

        // the library codes invalid values as float 999
        private double EditInvalidValues(double value)
        {
            if (value > 200) return double.NaN;
            return value;
        }
    }
}

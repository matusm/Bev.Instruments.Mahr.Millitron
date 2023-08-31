namespace GBC
{
    public class DummyThermometer : IThermoHygrometer
    {
        public string InstrumentID => "Dummy ThermoHygrometer";
        public double GetHumidity() => 20;
        public double GetTemperature() => 50;
    }
}

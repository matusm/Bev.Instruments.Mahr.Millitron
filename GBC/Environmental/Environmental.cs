using At.Matus.DataSeriesPod;

namespace GBC
{
    public class Environmental
    {
        private readonly IThermoHygrometer transmitter;
        private readonly DataSeriesPod temperature = new DataSeriesPod();
        private readonly DataSeriesPod humidity = new DataSeriesPod();

        public Environmental(IThermoHygrometer transmitter)
        {
            this.transmitter = transmitter;
        }

        public double Temperature => temperature.AverageValue;
        public double TemperatureScatter => temperature.Range / 2;
        public double TemperatureDrift => temperature.MostRecentValue - temperature.FirstValue;
        public double Humidity => humidity.AverageValue;
        public double HumidityScatter => humidity.Range / 2;
        public int SampleSize => (int)temperature.SampleSize;
        public string TransmitterID => transmitter.InstrumentID;

        public void Update()
        {
            temperature.Update(transmitter.GetTemperature());
            humidity.Update(transmitter.GetHumidity());
        }

        public void Restart()
        {
            temperature.Restart();
            humidity.Restart();
        }
    }
}

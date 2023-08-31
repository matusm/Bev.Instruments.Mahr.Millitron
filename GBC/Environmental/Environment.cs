using At.Matus.DataSeriesPod;

namespace GBC
{
    public class Environment
    {
        private readonly IThermoHygrometer transmitter;
        private readonly DataSeriesPod temperature = new DataSeriesPod();
        private readonly DataSeriesPod humidity = new DataSeriesPod();

        public Environment(IThermoHygrometer transmitter)
        {
            this.transmitter = transmitter;
        }

        public double Temperature => temperature.AverageValue;
        public double Humidity => humidity.AverageValue;
        public double TemperatureRange => temperature.Range;
        public double TemperatureDrift => temperature.MostRecentValue - temperature.FirstValue;
        public int SampleSize => (int)temperature.SampleSize;

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

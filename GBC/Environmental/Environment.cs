using At.Matus.StatisticPod;

namespace GBC
{
    public class Environment
    {
        private readonly IThermoHygrometer transmitter;
        private readonly StatisticPod temperature = new StatisticPod();
        private readonly StatisticPod humidity = new StatisticPod();

        public Environment(IThermoHygrometer transmitter)
        {
            this.transmitter = transmitter;
        }

        public double Temperature => temperature.AverageValue;
        public double Humidity => humidity.AverageValue;
        public double TemperatureRange => temperature.Range;
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

using Bev.Instruments.Conrad.Relais;

namespace Bev.Instruments.Mahr.Millitron
{
    public class ConradProbeMover : IProbeMover
    {
        private readonly ConradRelais conradRelais;

        public ConradProbeMover(string comPort)
        {
            conradRelais = new ConradRelais(comPort);
            RelaisNumber = 1; // default
        }

        public int RelaisNumber { get; set; }

        public void DropProbe() => conradRelais.Off(RelaisNumber);

        public void LiftProbe() => conradRelais.On(RelaisNumber);
    }
}

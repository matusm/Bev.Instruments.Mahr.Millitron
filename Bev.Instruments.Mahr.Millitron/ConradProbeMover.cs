using Bev.Instruments.Conrad.Relais;

namespace Bev.Instruments.Mahr.Millitron
{
    public class ConradProbeMover : IProbeMover
    {
        private readonly ConradRelais conradRelais;

        public ConradProbeMover(string comPort, int relaisNumber)
        {
            conradRelais = new ConradRelais(comPort);
            RelaisNumber = relaisNumber;
        }

        public int RelaisNumber { get; set; }

        public void DropProbe() => conradRelais.Off(RelaisNumber);

        public void LiftProbe() => conradRelais.On(RelaisNumber);
    }
}

namespace Bev.Instruments.Mahr.Millitron
{
    public class NullProbeMover : IProbeMover
    {
        public void DropProbe() => NOP();

        public void LiftProbe() => NOP();

        private void NOP() { }
    }
}

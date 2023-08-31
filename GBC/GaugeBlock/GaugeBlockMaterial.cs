namespace GBC
{
    public class GaugeBlockMaterial
    {
        public GaugeBlockMaterial(string symbol,
                                  string designationDE,
                                  string designationEN,
                                  double alpha,
                                  double compression)
        {
            Symbol = symbol.ToUpper();
            DesignationDE = designationDE;
            DesignationEN = designationEN;
            Alpha = alpha;
            Compression = compression;
        }

        public string Symbol { get; }
        public string DesignationDE { get; }
        public string DesignationEN { get; }
        public double Alpha { get; }
        public double Compression { get; }

        public bool IsMaterial(string symbol) => Symbol.Contains(symbol);

    }
}

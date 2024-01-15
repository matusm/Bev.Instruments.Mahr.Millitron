namespace JsonMockup
{
    public class MeasurementResultPoco
    {
        public string MrID { get; set; }
        public string Label { get; set; }
        public string Abbreviation { get; set; }
        public ComplexReal ComplexReal { get; set; }
    }

    public class ComplexReal
    {
        public string Unit { get; set; }
        public Value Value { get; set; }
    }

    public class Value
    {
        public int DecimalPositions { get; set; }
        public double Float { get; set; }
        public int Sign { get; set; }
    }
}
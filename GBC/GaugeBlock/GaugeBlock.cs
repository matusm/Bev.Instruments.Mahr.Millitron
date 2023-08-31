namespace GBC
{
    public class GaugeBlock
    {
        public GaugeBlock(string designation, string manufacturer, double nominalLength, GaugeBlockMaterial material, double? deviation)
        {
            Designation = designation;
            Manufacturer = manufacturer;
            NominalLength = nominalLength;
            Material = material;
            CenterDeviation = deviation;
            V = null;
            F_o = null;
            F_u = null;
            F_max = null;
            TemperatureCorrection = null;
            ElasticCorrection = null;
            IsModified = false;
            Temperature = referenceTemperature;
        }

        public GaugeBlock(string designation, string manufacturer, double nominalLength, GaugeBlockMaterial material) : this(designation, manufacturer, nominalLength, material, null) { }

        public string Designation { get; }
        public string Manufacturer { get; }
        public double NominalLength { get; }                // in mm
        public GaugeBlockMaterial Material { get; }
        public double Temperature { get; set; }
        public string MaterialBezeichnung => Material.DesignationDE;
        public string MaterialType => Material.DesignationEN; 
        public double? CenterDeviation { get; set; }        // in µm
        public double? V { get; set; }                      // in µm
        public double? F_o { get; set; }                    // in µm
        public double? F_u { get; set; }                    // in µm
        public double? F_max { get; set; }                  // in µm
        public double? TemperatureCorrection { get; set; }  // in µm
        public double? ElasticCorrection { get; set; }      // in µm
        public bool IsModified { get; set; }
        public bool IsCalibrated => CenterDeviation != null;

        // treat "this" as the gauge block to be calibrated
        public void CalibrateWith(GaugeBlock gbN, double d)
        {
            if (!gbN.IsCalibrated) 
                return;
            if (gbN.NominalLength != NominalLength) // this is dangerous
                return;
            ElasticCorrection = Material.Compression - gbN.Material.Compression;
            TemperatureCorrection = 1000.0 * (NominalLength * (1.0e6 + gbN.Material.Alpha * (gbN.Temperature - referenceTemperature)) / (1.0e6 + Material.Alpha * (Temperature - referenceTemperature)) - NominalLength);
            CenterDeviation = gbN.CenterDeviation + d + ElasticCorrection + TemperatureCorrection;
        }

        private const double referenceTemperature = 20;

    }
}

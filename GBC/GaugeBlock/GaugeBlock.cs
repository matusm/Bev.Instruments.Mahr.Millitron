using System;

namespace GBC
{
    public class GaugeBlock
    {
        public GaugeBlock(string manufacturer, string designation, double nominalLength, GaugeBlockMaterial material, double? deviation)
        {
            Designation = designation;
            Manufacturer = manufacturer;
            NominalLength = nominalLength;
            Material = material;
            CenterDeviation = deviation;
            V = null;
            Fo = null;
            Fu = null;
            TemperatureCorrection = null;
            ElasticCorrection = null;
            IsModified = false;
            Temperature = referenceTemperature;
        }

        public GaugeBlock(string manufacturer, string designation, double nominalLength, GaugeBlockMaterial material) : this(manufacturer, designation, nominalLength, material, null) { }

        public GaugeBlock() { } // never ever try to use this object!

        public string Designation { get; }
        public string Manufacturer { get; }
        public double NominalLength { get; }                // in mm
        public GaugeBlockMaterial Material { get; }
        public double Temperature { get; set; }
        public string MaterialBezeichnung => Material.DesignationDE;
        public string MaterialType => Material.DesignationEN; 
        public double? CenterDeviation { get; set; }        // in µm
        public double? V { get; private set; }              // in µm
        public double? Fo { get; private set; }             // in µm
        public double? Fu { get; private set; }             // in µm
        public double? Fmax => CalculateFmax();             // in µm
        public double? TemperatureCorrection { get; private set; }  // in µm
        public double? ElasticCorrection { get; private set; }      // in µm
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

        public void AddVariationData(VariationData data)
        {
            V = data.V/1000;
            Fo = data.Fo/1000;
            Fu = data.Fu/1000;
        }

        private double? CalculateFmax()
        {
            if (Fu == null || Fo == null || CenterDeviation == null)
                return null;
            return Math.Max(Math.Abs((double)CenterDeviation + (double)Fo), Math.Abs((double)CenterDeviation - (double)Fu));
        }

        private const double referenceTemperature = 20;

    }
}

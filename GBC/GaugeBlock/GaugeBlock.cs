using System;
using System.Text;

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
            F_o = null;
            F_u = null;
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
        public double? F_o { get; private set; }            // in µm
        public double? F_u { get; private set; }            // in µm
        public double? F_max => CalculateFmax();            // in µm
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
            F_o = data.Fo/1000;
            F_u = data.Fu/1000;
        }

        public override string ToString() => DebugToString();

        private double? CalculateFmax()
        {
            if (F_u == null || F_o == null || CenterDeviation == null)
                return null;
            return Math.Max(Math.Abs((double)CenterDeviation + (double)F_o), Math.Abs((double)CenterDeviation - (double)F_u));
        }

        private string DebugToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Endmaß:     {Designation} ({Manufacturer})");
            sb.AppendLine($"Nennlänge:  {NominalLength} mm");
            sb.AppendLine($"Material:   {MaterialBezeichnung}");
            sb.AppendLine($"Abweichung: {CenterDeviation} µm");
            sb.AppendLine($"AbwSpanne:  {V} µm");
            sb.AppendLine($"Temperatur: {Temperature} °C");
            return sb.ToString();
        }

        private const double referenceTemperature = 20;

    }
}

using System;

namespace GBC
{
    public class SessionData
    {
        private readonly MaterialsCollection materials;

        public SessionData()
        {
            materials = new MaterialsCollection();
        }

        public string Auftrag;
        public string TZahl;
        public string Kommentar;
        public string Beobachter;

        private string PHersteller;
        private string PBezeichnung;
        private string PMaterialSymbol;
        private double PNominalLength = double.NaN;

        private string NHersteller;
        private string NBezeichnung;
        private string NMaterialSymbol;
        private double NNominalLength = double.NaN;
        private double NDeviation = double.NaN;

        public void QuerySessionData()
        {
            if (IsEmpty(Auftrag)) Auftrag = PromptForText("Auftrag:    ");
            if (IsEmpty(TZahl)) TZahl = PromptForText("T-Zahl:     ");
            if (IsEmpty(Kommentar)) Kommentar = PromptForText("Kommentar:  ");
            if (IsEmpty(Beobachter)) Beobachter = PromptForText("Beobachter: ");
        }

        public GaugeBlock QueryTestBlock()
        {
            if (IsEmpty(PHersteller)) PHersteller = PromptForText("Prüfling (Hersteller):        ");
            if (IsEmpty(PBezeichnung)) PBezeichnung = PromptForText("Prüfling (Bezeichnung):       ");
            if (IsEmpty(PMaterialSymbol)) PMaterialSymbol = PromptForText("Material, Prüfling {S|K|H|Q}: ").ToUpper();
            if (IsEmpty(PNominalLength)) PNominalLength = PromptForNumber("Nennlänge (mm):               ");
            GaugeBlockMaterial material = materials.GetMaterial(PMaterialSymbol);
            return new GaugeBlock(PHersteller, PBezeichnung, PNominalLength, material);
        }

        //  QueryTestBlock() must be called in advance!
        public GaugeBlock QueryStandardBlock()
        {
            if (IsEmpty(NHersteller)) NHersteller = PromptForText("Normal (Hersteller):          ");
            if (IsEmpty(NBezeichnung)) NBezeichnung = PromptForText("Normal (Bezeichnung):         ");
            if (IsEmpty(NMaterialSymbol)) NMaterialSymbol = PromptForText("Material, Normal {S|K|H|Q}:   ").ToUpper();
            NNominalLength = PNominalLength; 
            if (IsEmpty(NDeviation)) NDeviation = PromptForNumber("Abweichung, Normal(µm):       ");
            GaugeBlockMaterial material = materials.GetMaterial(NMaterialSymbol);
            return new GaugeBlock(NHersteller, NBezeichnung, NNominalLength, material, NDeviation);
        }

        private bool IsEmpty(string str) => string.IsNullOrWhiteSpace(str);

        private bool IsEmpty(double value) => double.IsNaN(value);

        private string PromptForText(string message)
        {
            Console.Write(message);
            string input = Console.ReadLine();
            return input.Trim();
        }

        private double PromptForNumber(string message)
        {
            string input = PromptForText(message);
            return ParseNumberTolerant(input);
        }

        private double ParseNumberTolerant(string input)
        {
            input = input.Replace(",", ".");
            double result = double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
    }
}

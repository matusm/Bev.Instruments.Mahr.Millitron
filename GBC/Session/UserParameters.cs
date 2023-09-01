using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBC.Properties;

namespace GBC
{
    public class UserParameters
    {
        private readonly Settings settings;
        private readonly MaterialsCollection materials;

        public UserParameters()
        {
            settings = new Settings();
            materials = new MaterialsCollection();
        }

        public string Auftrag;
        public string TZahl;
        public string Kommentar;
        public string Beobachter;

        public string PHersteller;
        public string PBezeichnung;
        public string PMaterialSymbol;
        public double PNominalLength = double.NaN;

        public string NHersteller;
        public string NBezeichnung;
        public string NMaterialSymbol;
        public double NNominalLength = double.NaN;
        public double NDeviation = double.NaN;

        public void QueryUserInput()
        {
            if (IsEmpty(Auftrag)) Auftrag = PromptForText("Auftrag:    ");
            if (IsEmpty(TZahl)) TZahl = PromptForText("T-Zahl:     ");
            if (IsEmpty(Kommentar)) Kommentar = PromptForText("Kommentar:  ");
            if (IsEmpty(Beobachter)) Beobachter = PromptForText("Beobachter: ");

            if (IsEmpty(PHersteller)) PHersteller = PromptForText("Prüfling (Hersteller):        ");
            if (IsEmpty(PBezeichnung)) PBezeichnung = PromptForText("Prüfling (Bezeichnung):       ");
            if (IsEmpty(PMaterialSymbol)) PMaterialSymbol = PromptForText("Material, Prüfling {S|K|H|Q}: ").ToUpper();
            if (IsEmpty(PNominalLength)) PNominalLength = PromptForNumber("Nennlänge (mm):               ");

            if (IsEmpty(NHersteller)) NHersteller = PromptForText("Normal (Hersteller):          ");
            if (IsEmpty(NBezeichnung)) NBezeichnung = PromptForText("Normal (Bezeichnung):         ");
            if (IsEmpty(NMaterialSymbol)) NMaterialSymbol = PromptForText("Material, Normal {S|K|H|Q}:   ").ToUpper();
            //if (IsEmpty(NNominalLength)) NNominalLength = PromptForNumber("Nennlänge (mm):                 ");
            if (IsEmpty(NDeviation)) NDeviation = PromptForNumber("Abweichung, Normal(µm):       ");

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

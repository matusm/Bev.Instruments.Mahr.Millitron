using System.Collections.Generic;

namespace GBC
{
    public class MaterialsCollection
    {

        public MaterialsCollection()
        {
            materials = new List<GaugeBlockMaterial>();
            PopulateList();
        }

        public GaugeBlockMaterial GetMaterial(string symbol)
        {
            foreach (var m in materials)
            {
                if (m.IsMaterial(symbol))
                    return m;
            }
            return nullMaterial;
        }

        private void PopulateList()
        {
            materials.Add(new GaugeBlockMaterial("S", "Stahl", "steel", 11.7, 0.144));
            materials.Add(new GaugeBlockMaterial("CK", "Keramik (PSZ)", "ceramic (PSZ)", 9.3, 0.149));
            materials.Add(new GaugeBlockMaterial("TWH", "Hartmetall (WC)", "tungsten carbide", 4.2, 0.085));
            materials.Add(new GaugeBlockMaterial("Q", "Quarzkristall", "quartz crystal", 8.0, 0.223)); // the CTE in c-direction
            materials.Add(new GaugeBlockMaterial("R", "Rubin", "ruby", 5.6, 0.109)); // the CTE in c-direction
            materials.Add(new GaugeBlockMaterial("Z", "Zerodur", "Zerodur", 0.05, 0.232));
            materials.Add(new GaugeBlockMaterial("I", "Silizium", "silicon", 4.2, 0.186));
            materials.Add(new GaugeBlockMaterial("M", "Zaubermaterial", "magic material", 0.0, 0.000));
        }

        private readonly List<GaugeBlockMaterial> materials;
        private readonly GaugeBlockMaterial nullMaterial = new GaugeBlockMaterial("X", "nicht spezifiziert", "not specified", 0, 0);


    }
}

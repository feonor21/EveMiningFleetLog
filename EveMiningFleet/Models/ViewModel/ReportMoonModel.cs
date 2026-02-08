using System.Collections.Generic;

namespace EveMiningFleet.Models.ViewModel
{
    public class ReportMoonModel
    {
        public decimal Reprocess { get; set; } = 85;
        public decimal Taxe { get; set; } = 0;
        public string Rapport { get; set; } = "";
        public bool sellvalue { get; set; } = true;

        public bool ConversionLancer { get; set; } = false;
        public bool ConversionOK { get; set; } = false;
        public Dictionary<string, RML_Coorporation> Coorporations = new Dictionary<string, RML_Coorporation>();

        public Dictionary<int, RML_Ore> Ores = new Dictionary<int, RML_Ore>();

        public double PriceTotal;
        public double PriceTAXE;
        public double Pricegain;
        public float VolumeTotal;
    }
    public class RML_Coorporation
    {
        public Dictionary<string, RML_character> Characters = new Dictionary<string, RML_character>();

        public double PriceTotal;
        public double PriceTAXE;
        public double Pricegain;
        public float VolumeTotal;

        public Dictionary<int, RML_Ore> Ores = new Dictionary<int, RML_Ore>();
    }
    public class RML_character
    {
        public Dictionary<int, RML_Ore> Ores = new Dictionary<int, RML_Ore>();

        public double PriceTotal;
        public double PriceTAXE;
        public double Pricegain;
        public float VolumeTotal;
    }
    public class RML_Ore
    {
        public string Name;
        public int QuantityTotal;
        public int QuantityTAXE;
        public int Quantitygain;
        public float VolumeTotal;
        public float VolumeOre;
        public double PriceTotal;
        public double PriceOre;
    }
}

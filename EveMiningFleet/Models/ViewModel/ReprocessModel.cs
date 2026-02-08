using System.Collections.Generic;
using EveMiningFleet.Entities.Tables;


namespace EveMiningFleet.Models.ViewModel
{
    public class ReprocessModel
    {
        public decimal ReprocessingEfficiency { get; set; } = 85;
        public decimal Buyback { get; set; } = 100;
        public string rapport { get; set; } = "";

        public bool ConversionLancer { get; set; } = false;
        public bool ConversionOK { get; set; } = false;

        public Dictionary<string, ReprocessOreModel> ReprocessOreModels = new Dictionary<string, ReprocessOreModel>();

        public Dictionary<int, ReprocessOreMaterialModel> ReprocessOreMaterialModels = new Dictionary<int, ReprocessOreMaterialModel>();
    }
    public class ReprocessOreModel
    {
        public Ore OreItem;
        public int idreprocess;
        public int quantity;

        public double PriceCompressedBuy { get; set; }
        public double PriceCompressedSell { get; set; }
        public double PriceRefinedBuy { get; set; }
        public double PriceRefinedSell { get; set; }
    }
    public class ReprocessOreMaterialModel
    {
        public DataPrice dataprice;
        public int Quantity;

        public double PriceRefinedBuy { get; set; }
        public double PriceRefinedSell { get; set; }
    }
}

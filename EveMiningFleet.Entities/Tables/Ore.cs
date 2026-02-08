using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Ore
    {
        public Ore()
        {
            Lastmininglogs = new HashSet<Lastmininglog>();
            Mininglogs = new HashSet<Mininglog>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public float? Volume { get; set; }
        public bool? Publish { get; set; }
        public bool? CanBeCompressed { get; set; }
        public int? QuantityForReprocess { get; set; }
        public int? IdCompressed { get; set; }
        public int? IdSkillOreReprocessing { get; set; }
        public double? PriceCompressedBuy { get; set; }
        public double? PriceCompressedSell { get; set; }
        public double? PriceRefinedBuy { get; set; }
        public double? PriceRefinedSell { get; set; }

        [JsonIgnore]
        public virtual ICollection<Lastmininglog> Lastmininglogs { get; set; }

        [JsonIgnore]
        public virtual ICollection<Mininglog> Mininglogs { get; set; }
    }
}

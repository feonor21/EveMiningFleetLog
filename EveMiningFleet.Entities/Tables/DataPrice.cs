using System.ComponentModel.DataAnnotations;

namespace EveMiningFleet.Entities.Tables
{
    public class DataPrice
    {
        [Key]
        public int TypeId { get; set; }
        public double PriceSell { get; set; }
        public double PriceBuy { get; set; }
        public string Name { get; set; }
    }
}

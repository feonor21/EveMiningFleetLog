using System;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Lastmininglog
    {
        public int CharacterId { get; set; }
        public virtual Character Character { get; set; }
        public int OreId { get; set; }
        public virtual Ore Ore { get; set; }
        public DateTime Date { get; set; }
        public long Quantity { get; set; }

    }
}

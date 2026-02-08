#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Mininglog
    {
        public int FleetCharacterId { get; set; }
        public virtual Fleetcharacter FleetCharacter { get; set; }
        public int OreId { get; set; }
        public virtual Ore Ore { get; set; }
        public int Quantity { get; set; }

    }
}

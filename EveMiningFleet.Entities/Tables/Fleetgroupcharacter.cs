#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Fleetgroupcharacter
    {
        public int Id { get; set; }
        public int FleetgroupId { get; set; }
        public virtual Fleetgroup Fleetgroup { get; set; }
        public int CharacterId { get; set; }
        public virtual Character Character { get; set; }

    }
}

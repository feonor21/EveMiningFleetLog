#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Fleettaxes
    {
        public int Id { get; set; }
        public int FleetId { get; set; }
        public virtual Fleet Fleet { get; set; }
        public int? CharacterId { get; set; }
        public virtual Character Character { get; set; }
        public float Taxe { get; set; }
        public string Name { get; set; }

    }
}

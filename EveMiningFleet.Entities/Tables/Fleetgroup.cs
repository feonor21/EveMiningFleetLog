using System.Collections.Generic;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Fleetgroup
    {
        public Fleetgroup()
        {
            Fleetgroupcharacters = new HashSet<Fleetgroupcharacter>();
        }

        public int Id { get; set; }
        public int FleetId { get; set; }
        public virtual Fleet Fleet { get; set; }

        public virtual ICollection<Fleetgroupcharacter> Fleetgroupcharacters { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Fleetcharacter
    {
        public Fleetcharacter()
        {
            Mininglogs = new HashSet<Mininglog>();
        }

        public int Id { get; set; }
        public int FleetId { get; set; }
        public virtual Fleet Fleet { get; set; }
        public int CharacterId { get; set; }
        public virtual Character Character { get; set; }
        public DateTime Join { get; set; }
        public DateTime? Quit { get; set; }
        public DateTime? LastRefresh { get; set; }

        public virtual ICollection<Mininglog> Mininglogs { get; set; }
    }
}

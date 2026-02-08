using System;
using System.Collections.Generic;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Fleet
    {
        public Fleet()
        {
            Fleetcharacters = new HashSet<Fleetcharacter>();
            Fleetgroups = new HashSet<Fleetgroup>();
            Fleettaxes = new HashSet<Fleettaxes>();
        }

        public int Id { get; set; }
        public DateTime Begin { get; set; }
        public DateTime? End { get; set; }
        public string JoinToken { get; set; }
        public DateTime? LastFullRefresh { get; set; }
        public int? ViewRight { get; set; }
        public int? Distribution { get; set; }
        public double? Reprocess { get; set; }

        public int CharacterId { get; set; }
        public virtual Character Character { get; set; }

        public int CorporationId { get; set; }
        public virtual Corporation Corporation { get; set; }

        public int AllianceId { get; set; }
        public virtual Alliance Alliance { get; set; }
        public string ReasonClose { get; set; }

        public virtual ICollection<Fleetcharacter> Fleetcharacters { get; set; }
        public virtual ICollection<Fleetgroup> Fleetgroups { get; set; }
        public virtual ICollection<Fleettaxes> Fleettaxes { get; set; }
    }
}

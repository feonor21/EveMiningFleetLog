using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace EveMiningFleet.Entities.Tables
{
    public partial class Character
    {
        public Character()
        {
            Fleetcharacters = new HashSet<Fleetcharacter>();
            Fleetgroupcharacters = new HashSet<Fleetgroupcharacter>();
            Fleets = new HashSet<Fleet>();
            Fleettaxes = new HashSet<Fleettaxes>();
            Lastmininglogs = new HashSet<Lastmininglog>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public int CorporationId { get; set; }
        public virtual Corporation Corporation { get; set; }

        public int AllianceId { get; set; }
        public virtual Alliance Alliance { get; set; }

        public int CharacterMainId { get; set; }
        public virtual Character CharacterMain { get; set; }
        public virtual ICollection<Fleetcharacter> Fleetcharacters { get; set; }
        public virtual ICollection<Fleetgroupcharacter> Fleetgroupcharacters { get; set; }
        public virtual ICollection<Fleet> Fleets { get; set; }
        public virtual ICollection<Fleettaxes> Fleettaxes { get; set; }
        public virtual ICollection<Lastmininglog> Lastmininglogs { get; set; }
    }
}

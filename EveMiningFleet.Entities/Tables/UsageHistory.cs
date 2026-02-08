using System;

namespace EveMiningFleet.Entities.Tables
{
    public partial class UsageHistory
    {
        public int Id { get; set; }
        public DateTime date { get; set; }
        public int? fleetactive { get; set; }
        public int? characteractif { get; set; }
    }
}

using System.Linq;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace EveMiningFleet.Logic.Repository
{
    public class FleetRepository : BaseRepository
    {
        public FleetRepository(EveMiningFleetContext _eveMiningFleetContext) : base(_eveMiningFleetContext)
        {

        }

        public IQueryable<Fleet> GetSimple()
        {
            return eveMiningFleetContext.Fleets.Include("Character").Include("Corporation").Include("Alliance").Include("Fleetcharacters.Character");
        }
        public IQueryable<Fleet> GetDetails()
        {
            return eveMiningFleetContext.Fleets.Include("Character").Include("Corporation").Include("Alliance").Include("Fleetcharacters.Character");
        }
    }
}

using System.Linq;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;

namespace Cron.Tasks
{
    public class HistoryFunction
    {
        public static void AddHistoryFleet()
        {
            int numberOfFleetActiv = 0;
            int CharacterActiv = 0;
            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                var tmplist = DatabaseContext.Fleets.Where(x => x.End == null);
                numberOfFleetActiv = tmplist.Count();
                foreach (Fleet FleetOpen in tmplist)
                {
                    CharacterActiv += DatabaseContext.Fleetcharacters.Count(fleetchar => fleetchar.FleetId == FleetOpen.Id && fleetchar.Quit == null);
                }
            }
        }
    }
}

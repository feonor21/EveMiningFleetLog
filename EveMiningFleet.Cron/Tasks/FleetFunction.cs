using System;
using System.Linq;
using EveMiningFleet.Entities;
using EveMiningFleet.Logic.Tools;

namespace Cron.Tasks
{
    public class FleetFunction
    {

        /// <summary>
        /// Purge des fleet inactives
        /// </summary>
        public static void purgeFleet()
        {
            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                var now = DateTime.Now.ToUniversalTime();

                const int oldTimeSetting = 60;
                var oldFleet = DatabaseContext.Fleets.Where(x => x.End == null && x.Begin.AddDays(oldTimeSetting) < now);

                const int InactiveTimeSetting = 3;
                var inactiveFleet = DatabaseContext.Fleets.Where(x => x.End == null && x.Begin.AddDays(InactiveTimeSetting) < now && x.LastFullRefresh.Value.AddDays(InactiveTimeSetting) < now);

                foreach (var fleet in oldFleet)
                {
                    fleet.End = DateTime.Now.ToUniversalTime();
                    fleet.ReasonClose = "fleet open too long. forced clos";
                    ClassLog.writeLog("purgeFleet => Fleet Closed for overtime (" + oldTimeSetting.ToString() + "Days). ID:" + fleet.Id.ToString());
                }

                foreach (var fleet in inactiveFleet)
                {
                    fleet.End = DateTime.Now.ToUniversalTime();
                    fleet.ReasonClose = "nobody mine for too long";
                    ClassLog.writeLog("purgeFleet => Fleet Closed for inactivity(" + InactiveTimeSetting.ToString() + "Days). ID:" + fleet.Id.ToString());
                }

                DatabaseContext.SaveChanges();
            }
        }




    }
}

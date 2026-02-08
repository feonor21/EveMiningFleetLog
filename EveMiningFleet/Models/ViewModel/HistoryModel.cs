using System.Collections.Generic;
using EveMiningFleet.Entities.Tables;

namespace EveMiningFleet.Models.ViewModel
{
    public class HistoryModel
    {
        public List<FleetHistoric> listFleet = new List<FleetHistoric>();

    }
    public class FleetHistoric
    {
        public Fleet fleet;
        public string CorpoName = "";
        public string AllianceName = "";
        public void completName()
        {

        }
    }
}

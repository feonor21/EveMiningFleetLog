using EveMiningFleet.Entities;

namespace EveMiningFleet.Logic.Repository
{
    public class BaseRepository
    {
        protected readonly EveMiningFleetContext eveMiningFleetContext;
        public BaseRepository(EveMiningFleetContext _eveMiningFleetContext)
        {
            eveMiningFleetContext = _eveMiningFleetContext;
        }
    }
}

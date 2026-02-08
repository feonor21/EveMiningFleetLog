using System.Linq;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace EveMiningFleet.Logic.Repository
{
    public class CharacterRepository : BaseRepository
    {
        public CharacterRepository(EveMiningFleetContext _eveMiningFleetContext) : base(_eveMiningFleetContext)
        {

        }

        public IQueryable<Character> GetSimple()
        {
            return eveMiningFleetContext.Characters.Include("Corporation").Include("Alliance");
        }
    }
}

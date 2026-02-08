using System.Linq;
using EveMiningFleet.Entities;

namespace EveMiningFleet.Models
{
    public class CharacterFunction
    {

        /// <summary>
        /// permet de recuper l'id de la fleet en cours du characte cibler
        /// </summary>
        /// <param name="IDCharacter">ID du character</param>
        /// <returns>0 no fleet , >0 fleetID</returns>
        public static int GetFleetIdOfCharacter(int IDCharacter)
        {
            int reponse = 0;
            using (EveMiningFleetContext DbContext = new EveMiningFleetContext())
            {
                var tempfleetchar = DbContext.Fleetcharacters.FirstOrDefault(x => x.CharacterId == IDCharacter && x.Quit == null && x.Fleet.End == null);
                if (tempfleetchar != null)
                    reponse = tempfleetchar.FleetId;
            }
            return reponse;
        }




    }
}

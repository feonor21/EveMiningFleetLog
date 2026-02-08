using System.Collections.Generic;
using System.Linq;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.EsiEve;

namespace Cron.Tasks
{
    public static class ScopeFunction
    {

        /// <summary>
        /// refresh tout les scopes de tout le monde
        /// </summary>
        public static void checkAllScope()
        {
            List<int> AllIdChar = new List<int>();

            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                foreach (Character character in DatabaseContext.Characters.Where(x => x.Token != ""))
                {
                    AllIdChar.Add(character.Id);
                }
            }

            foreach (var item in AllIdChar)
            {
                checkOneScope(item);
            }

            AllIdChar = null;
        }
        /// <summary>
        /// Verifie si l'utilisateur est encore accessible
        /// </summary>
        public static bool checkOneScope(int IDcharacter)
        {
            bool response = true;
            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                Character character = DatabaseContext.Characters.FirstOrDefault(x => x.Id == IDcharacter);
                if (character == null)
                    return false;

                if (character.Token != "")
                {
                    EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();
                    var countertest = 0;

                    //TODO: me parait pas propre quand meme .....
                    do
                    {
                        tmpEsiConnection.RefreshConnection(character.RefreshToken).Wait();
                        countertest++;
                    } while (countertest < 5 && tmpEsiConnection.authorizedCharacterData == null);

                    if (tmpEsiConnection.authorizedCharacterData == null)
                    {
                        character.Token = "";
                        character.RefreshToken = "";
                        character.CharacterMainId = character.Id;
                        response = false;
                    }
                    else
                    {
                        if (character.CharacterMainId == 0)
                            character.CharacterMainId = character.Id;
                    }
                    DatabaseContext.SaveChanges();
                }
                else if (character.Token == "" && character.CharacterMainId != character.Id)
                {
                    //bha on avais deja plus les droit sur le perso , on reset sont mainID
                    character.CharacterMainId = character.Id;
                    response = false;
                    DatabaseContext.SaveChanges();
                }
                else
                {
                    response = false;
                }
            }
            return response;
        }

    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.EsiEve;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EveMiningFleet.Controllers
{
    public class LoginController : BaseController
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IWebHostEnvironment _env;

        public LoginController(ILogger<LoginController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }


        public IActionResult UnLogin()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult JoinFleetByLink(int JoinID, string JoinToken)
        {
            if (JoinToken == null)
                throw new Exception("The url link is not correctly formatted. Check the joinfleet url link. you dont have -JoinToken-");
            if (JoinID == 0)
                throw new Exception("The url link is not correctly formatted. Check the joinfleet url link. you dont have -JoinID-");


            HttpContext.Session.SetInt32("JoinFleetByLink_JoinID", JoinID);
            HttpContext.Session.SetString("JoinFleetByLink_JoinToken", JoinToken);
            return RedirectToAction("LoginCCP");
        }

        public IActionResult LoginCCP()
        {
            EveEsiConnexion tmpesi = new EveEsiConnexion();
            string response = tmpesi.GetUrlConnection();
            tmpesi = null;

            return Redirect(response);
        }
        public ActionResult SetTimeZone(int timeZoneOffset)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.TimeZone != timeZoneOffset)
                {
                    sessionUtility.TimeZone = timeZoneOffset;
                    HttpContext.Session.Set(_Constante._SessionUtility, sessionUtility.ToByteArray());
                }
                // timeZoneOffset contient la valeur envoyée depuis le client

                // Vous pouvez maintenant utiliser timeZoneOffset comme bon vous semble
                // Par exemple, vous pourriez l'enregistrer dans une base de données ou effectuer une autre action en fonction de la zone horaire.

                return Content("Données reçues avec succès !");
            }
            catch (Exception ex)
            {
                // Gérez les exceptions ici si nécessaire
                return Content("Erreur lors du traitement des données : " + ex.Message);
            }
        }
        public async Task<IActionResult> CallbackCCP(string code)
        {
            if (code == "")
                return RedirectToAction("error400", "Messages");

            EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();

            await tmpEsiConnection.GetToken(code);
            await tmpEsiConnection.ConnectCharCCP();

            Character characterConnexion;
            //recuperation de l'alliance de la corp et du joueur

            //on recupere la corp du joueur qui viens de ce connecter si on le connais pas on l'insert dans la table coorp
            Corporation corporation = DbContext.Corporations.FirstOrDefault((x) => x.Id == tmpEsiConnection.authorizedCharacterData.CorporationID);
            if (corporation == null)
            {
                corporation = new Corporation();
                corporation.Id = tmpEsiConnection.authorizedCharacterData.CorporationID;
                corporation.Name = EveMiningFleet.Logic.EsiEve.EsiCorporation.GetName(corporation.Id);
                DbContext.Corporations.Add(corporation);
            }

            //on recupere l'alliance du joueur qui viens de ce connecter si on le connais pas on l'insert dans la table alliance
            Alliance alliance = DbContext.Alliances.FirstOrDefault((x) => x.Id == tmpEsiConnection.authorizedCharacterData.AllianceID);
            if (alliance == null)
            {
                alliance = new Alliance();
                alliance.Id = tmpEsiConnection.authorizedCharacterData.AllianceID;
                alliance.Name = EveMiningFleet.Logic.EsiEve.EsiAlliance.GetName(alliance.Id);
                DbContext.Alliances.Add(alliance);
            }

            //on recupere enfin le joueur et si jamais on le connais pas on l'insert
            characterConnexion = DbContext.Characters.FirstOrDefault((x) => x.Id == tmpEsiConnection.authorizedCharacterData.CharacterID);
            if (characterConnexion == null)
            {
                characterConnexion = new Character();

                characterConnexion.Id = tmpEsiConnection.authorizedCharacterData.CharacterID;
                characterConnexion.CharacterMainId = tmpEsiConnection.authorizedCharacterData.CharacterID;
                characterConnexion.CharacterMain = characterConnexion;
                DbContext.Characters.Add(characterConnexion);
            }

            characterConnexion.AllianceId = tmpEsiConnection.authorizedCharacterData.AllianceID;
            characterConnexion.CorporationId = tmpEsiConnection.authorizedCharacterData.CorporationID;

            DbContext.SaveChanges();

            characterConnexion = DbContext.Characters.Include("Corporation").Include("Alliance").Include("CharacterMain").FirstOrDefault((x) => x.Id == tmpEsiConnection.authorizedCharacterData.CharacterID);
            //on met a jours les informations du player.
            characterConnexion.Name = tmpEsiConnection.authorizedCharacterData.CharacterName;
            characterConnexion.Token = tmpEsiConnection.ssoToken.AccessToken;
            characterConnexion.RefreshToken = tmpEsiConnection.ssoToken.RefreshToken;

            DbContext.SaveChanges();

            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

            //si la session est vierge(mainID =0 alors on vas regarder si le perso est un alt
            if (sessionUtility.MainCharacterID == 0)
                sessionUtility.MainCharacterID = characterConnexion.CharacterMainId;

            sessionUtility.MainCoorpID = characterConnexion.CharacterMain.CorporationId;
            sessionUtility.MainAllianceID = characterConnexion.CharacterMain.AllianceId;


            //ecriture du mainID du charactere si le mainID n'est pas le bon mainID
            if (sessionUtility.MainCharacterID != characterConnexion.CharacterMainId)
            {
                Character tmp = DbContext.Characters.FirstOrDefault((x) => x.Id == characterConnexion.Id);
                tmp.CharacterMainId = sessionUtility.MainCharacterID;
                DbContext.SaveChanges();
            }

            // on met a jour les alliance et les corps et les persos accessible.
            var childchar = DbContext.Characters.Include("Corporation").Include("Alliance").Where((x) => x.CharacterMainId == sessionUtility.MainCharacterID && x.Token != "");
            sessionUtility.ConstructList(childchar);


            HttpContext.Session.Set(_Constante._SessionUtility, sessionUtility.ToByteArray());

            //on regarde si on essayer de rejoindre via un lien
            if (HttpContext.Session.GetInt32("JoinFleetByLink_JoinID") != null && HttpContext.Session.GetInt32("JoinFleetByLink_JoinToken") != null)
                return RedirectToAction("Join", "Fleets",
                    new
                    {
                        FleetId = HttpContext.Session.GetInt32("JoinFleetByLink_JoinID"),
                        Token = (string)HttpContext.Session.GetString("JoinFleetByLink_JoinToken"),
                        CharacterId = characterConnexion.Id
                    });
            else
                return RedirectToAction("Profil", "Home");
        }

        public IActionResult toogledarkmode()
        {
            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

            sessionUtility.darkmode = !sessionUtility.darkmode;

            HttpContext.Session.Set(_Constante._SessionUtility, sessionUtility.ToByteArray());

            return RedirectToAction("Profil", "Home");
        }


    }
}
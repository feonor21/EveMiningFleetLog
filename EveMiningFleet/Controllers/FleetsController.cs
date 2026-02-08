using System;
using System.Linq;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EveMiningFleet.Controllers
{
    public class FleetsController : BaseController
    {
        private readonly ILogger<FleetsController> _logger;
        private readonly IWebHostEnvironment _env;

        public FleetsController(ILogger<FleetsController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public Fleet FleetAccess(int FleetId, bool partial)
        {
            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));
            Fleet fleet;
            fleet = DbContext.Fleets.Include("Character").
                Include("Fleettaxes.Character").
                Include("Fleetcharacters.Mininglogs.Ore").
                Include("Fleetcharacters.Character.Corporation").
                Include("Fleetcharacters.Character.Alliance").
                Include("Fleetgroups.Fleetgroupcharacters.Character.Corporation").
                Include("Fleetgroups.Fleetgroupcharacters.Character.Alliance").
                Include("Fleetgroups.Fleetgroupcharacters.Character.CharacterMain.Corporation").
                Include("Fleetgroups.Fleetgroupcharacters.Character.CharacterMain.Alliance").
                FirstOrDefault(x => x.Id == FleetId);

            if (fleet == null)
                throw new Exception("Fleet number :" + FleetId + " not exist.");

            bool canaccess = false;


            if (canaccess == false && fleet.ViewRight == 3)
            {
                //public access
                canaccess = true;
            }
            else
            {
                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("This fleet(" + FleetId + ") is not public, You must authentified for access to details of this fleet.");


                //basic access. you mine in, so you can see it
                var allCharacterFromFleet = EveMiningFleet.Models.ViewModel.FleetRapportFunction.GetAllCharacterFromFleet(fleet).ToList();
                foreach (var cursor in allCharacterFromFleet)
                {
                    canaccess = sessionUtility.AllCharacter.Any(x => x.Key == cursor.Id);
                    if (canaccess)
                        break;
                }

                // fleet coorpo
                if (canaccess == false && (fleet.ViewRight == 1))
                    canaccess = sessionUtility.AllCoorp.ContainsKey(fleet.CorporationId);

                // fleet alliance
                if (canaccess == false && (fleet.ViewRight == 2))
                    canaccess = sessionUtility.AllAlliance.ContainsKey(fleet.AllianceId);

                //admin access god view . not good but necessary for maintenance and debug
                if (canaccess == false && sessionUtility.MainCharacterID == 96852613 && sessionUtility.AllCharacter.ContainsKey(2112721651))
                {
                    canaccess = true;
                    if (!partial)
                        ClassLog.writeLog("WatchFleet, access GodView given for fleet ID: " + FleetId.ToString());
                }

                if (canaccess == false)
                {
                    switch (fleet.ViewRight)
                    {
                        case null:
                        case 0:
                            throw new Exception("This fleet(" + FleetId + ") is private, You can't access to details of this fleet.");
                        case 1:
                            throw new Exception("This fleet(" + FleetId + ") limited to coorporation, You can't access to details of this fleet.");
                        case 2:
                            throw new Exception("This fleet(" + FleetId + ") is limited to alliance, You can't access to details of this fleet.");
                        case 3:
                            throw new Exception("WTF. its not possible. its an public fleet. but you dont have access. contact admin on the discord.");
                        default:
                            throw new Exception("WTF. How did you get here. contact admin on the discord.");
                    }
                }
            }

            return fleet;

        }
        public IActionResult Details(int FleetId)
        {
            try
            {
                Fleet fleet = FleetAccess(FleetId, false);

                if (TempData["EditTaxe"] != null)
                    ViewBag.EditFleetTaxeId = (Fleettaxes)TempData["EditTaxe"];
                TempData["EditTaxe"] = null;

                ViewBag.Title = "Détails of Fleets";
                return View("ReportFleet", fleet);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Details Fleets \n" + ex.Message;
                ViewBag.LastUrl = Url.Action("Details", "Fleets", new { FleetId = FleetId }, "https");
                return View("ErrorFleet");
            }
        }
        public IActionResult DetailsPartial(int FleetId)
        {
            try
            {
                Fleet fleet = FleetAccess(FleetId, true);
                return PartialView("ReportFleetPartial", fleet);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Details Fleets \n" + ex.Message;
                return PartialView("ErrorFleetPartial");
            }
        }

        public IActionResult Create(int CharacterID)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));


                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");

                if (!sessionUtility.AllCharacter.ContainsKey(CharacterID))
                    throw new Exception("You cannot join with a character that is not in your authenticated alt");


                Fleetcharacter tmpfleetchar;
                Fleet newfleet;

                tmpfleetchar = DbContext.Fleetcharacters.Include("Fleet").OrderByDescending(x => x.Id).FirstOrDefault(x => x.CharacterId == CharacterID && x.Fleet.End == null && x.Quit == null);
                if (tmpfleetchar != null)
                    throw new Exception(sessionUtility.AllCharacter[CharacterID] + " are already in a mining fleet, cannot join another fleet.");

                Character character = DbContext.Characters.FirstOrDefault((x) => x.Id == CharacterID);
                newfleet = new Fleet();
                newfleet.CharacterId = CharacterID;
                newfleet.CorporationId = character.CorporationId;
                newfleet.AllianceId = character.AllianceId;
                newfleet.Begin = DateTime.Now.ToUniversalTime();
                newfleet.Reprocess = 86;
                newfleet.JoinToken = System.IO.Path.GetRandomFileName().Replace(".", "");
                newfleet.LastFullRefresh = DateTime.Now.ToUniversalTime();
                DbContext.Fleets.Add(newfleet);
                DbContext.SaveChanges();

                ClassLog.writeLog("New Fleet Created. ID: " + newfleet.Id.ToString());
                return RedirectToAction("Join", new { FleetId = newfleet.Id, Token = newfleet.JoinToken, CharacterID = CharacterID });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Create Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }
        public IActionResult Close(int FleetId)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");

                Fleet fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleet.CharacterId))
                    throw new Exception("You are not the leader of this fleet");

                fleet.End = DateTime.Now.ToUniversalTime();
                DbContext.SaveChanges();

                ClassLog.writeLog("Fleet Closed by player. ID: " + FleetId.ToString());
                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Close Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }

        public IActionResult Join(int FleetId, string Token, int CharacterID)
        {
            HttpContext.Session.Remove("JoinFleetByLink_JoinID");
            HttpContext.Session.Remove("JoinFleetByLink_JoinToken");

            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (!sessionUtility.AllCharacter.ContainsKey(CharacterID))
                    throw new Exception("You must be authentified for Join fleet.");

                Fleet fleet;

                fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");

                if (fleet.End != null)
                    throw new Exception("Fleet number :" + FleetId + " is closed, you can't join her.");

                if (fleet.JoinToken != Token)
                    throw new Exception("Fleet number :" + FleetId + " your token for joining fleet is invalid");


                joinFunc(sessionUtility, FleetId, CharacterID);

                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Join Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }

        public IActionResult JoinAll(int FleetId, string Token, int MainCharId)
        {
            HttpContext.Session.Remove("JoinFleetByLink_JoinID");
            HttpContext.Session.Remove("JoinFleetByLink_JoinToken");

            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (!sessionUtility.AllCharacter.ContainsKey(MainCharId))
                    throw new Exception("You must be authentified for Join fleet.");

                Fleetcharacter tmpfleetchar;
                Fleet fleet;

                fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");

                if (fleet.End != null)
                    throw new Exception("Fleet number :" + FleetId + " is closed, you can't join her.");

                if (fleet.JoinToken != Token)
                    throw new Exception("Fleet number :" + FleetId + " your token for joining fleet is invalid");


                foreach (var CharacterID in sessionUtility.AllCharacter.Keys)
                {
                    try
                    {
                        joinFunc(sessionUtility, FleetId, CharacterID);
                    }
                    catch (Exception ex)
                    {
                        ClassLog.writeLog("JoinAll, Fleet number :" + FleetId + ", MainCharId :" + MainCharId + " Error:");
                        ClassLog.writeException(ex);
                    }
                }

                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Join Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }

        private void joinFunc(_SessionUtility sessionUtility, int FleetId, int CharacterID)
        {

            Fleetcharacter tmpfleetchar;

            tmpfleetchar = DbContext.Fleetcharacters.Include("Fleet").OrderByDescending(x => x.Id).FirstOrDefault(x => x.CharacterId == CharacterID && x.Quit == null && x.Fleet.End == null);
            if (tmpfleetchar != null && tmpfleetchar.FleetId == FleetId)
                return;

            if (tmpfleetchar != null)
                throw new Exception(sessionUtility.AllCharacter[CharacterID] + " are already in a mining fleet, You cannot join another fleet.");

            tmpfleetchar = new Fleetcharacter();
            tmpfleetchar.FleetId = (int)FleetId;
            tmpfleetchar.CharacterId = CharacterID;
            tmpfleetchar.Join = DateTime.Now.ToUniversalTime();
            DbContext.Fleetcharacters.Add(tmpfleetchar);
            DbContext.SaveChanges();

            tmpfleetchar = DbContext.Fleetcharacters.Include("Fleet").Include("Character").First(x => x.Id == tmpfleetchar.Id);


            //on supprime les anciens liens
            Fleetgroupcharacter fleetgroupcharacter = DbContext.Fleetgroupcharacters.Include("Fleetgroup.Fleetgroupcharacters").FirstOrDefault(x => x.Fleetgroup.FleetId == FleetId && x.CharacterId == CharacterID);
            if (fleetgroupcharacter == null)
            {
                //on recupere le group parent si il existe
                Fleetgroup fleetgroup = DbContext.Fleetgroups.Include("Fleetgroupcharacters.Character").FirstOrDefault(x => x.FleetId == FleetId && x.Fleetgroupcharacters.Any(y => y.Character.CharacterMainId == sessionUtility.MainCharacterID));

                //on creer le group parent si y as besoin
                if (fleetgroup == null)
                {
                    fleetgroup = new Fleetgroup() { FleetId = FleetId };
                    DbContext.Fleetgroups.Add(fleetgroup);
                }
                //on creer l'enfant
                fleetgroupcharacter = new Fleetgroupcharacter()
                {
                    Fleetgroup = fleetgroup,
                    CharacterId = CharacterID
                };
                DbContext.Fleetgroupcharacters.Add(fleetgroupcharacter);

                DbContext.SaveChanges();
            }
            ClassLog.writeLog("JoinFleet, " + sessionUtility.AllCharacter[CharacterID].ToString() + " join fleet ID: " + FleetId.ToString());
        }


        public IActionResult Quit(int FleetId, int CharacterID)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");


                Fleet fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");
                if (fleet.End != null)
                    throw new Exception("Fleet number :" + FleetId + " Already closed, so you can't quit her.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleet.CharacterId) && !sessionUtility.AllCharacter.ContainsKey(CharacterID))
                    throw new Exception("You cannot kick a character if you are not the FC");


                Character character = DbContext.Characters.Include("Fleetcharacters").FirstOrDefault(x => x.Id == CharacterID);
                if (character == null)
                    throw new Exception("the character :" + CharacterID + " is unknown.");



                Fleetcharacter fleetcharacter = character.Fleetcharacters.FirstOrDefault(x => x.FleetId == FleetId && x.Quit == null);

                if (fleetcharacter == null)
                    throw new Exception("the character :" + character.Name + " is not in fleet " + FleetId + ".");

                fleetcharacter.Quit = DateTime.Now.ToUniversalTime();
                DbContext.SaveChanges();


                ClassLog.writeLog("QuitFleet, " + sessionUtility.AllCharacter[CharacterID].ToString() + " quit fleet ID: " + FleetId.ToString());
                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Quit Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }


        public IActionResult EditOption(int FleetId, int Access, int Distribution, int Reprocess)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("You must be authentified for Edit option of an fleet.");

                Fleet fleet;

                fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleet.CharacterId))
                    throw new Exception("You havent fleet where you are the leader");

                fleet.ViewRight = Access;
                fleet.Distribution = Distribution;
                fleet.Reprocess = Reprocess;

                DbContext.SaveChanges();

                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Join Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }

        public IActionResult TaxeAdd(int FleetId)
        {

            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");

                Fleet fleet = DbContext.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (fleet == null)
                    throw new Exception("Fleet number :" + FleetId + " not exist.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleet.CharacterId))
                    throw new Exception("You cannot add taxe if you are not the leader");

                Fleettaxes fleettaxe = new Fleettaxes
                {
                    FleetId = (int)FleetId
                };
                DbContext.Fleettaxes.Add(fleettaxe);
                DbContext.SaveChanges();

                return RedirectToAction("Details", new { FleetId = FleetId });
            }
            catch (Exception ex)
            {
                @ViewBag.Message = "AddTaxeLogistic Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }
        public IActionResult TaxeEdit(int FleetTaxeId, string FleetTaxeName, float FleetTaxeTaxe, int? FleetTaxeCharacterId)
        {

            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");

                Fleettaxes fleettaxe;
                fleettaxe = DbContext.Fleettaxes.Include("Fleet").FirstOrDefault(x => x.Id == FleetTaxeId);
                if (fleettaxe == null)
                    throw new Exception("Fleet taxe :" + FleetTaxeId + " not exist.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleettaxe.Fleet.CharacterId))
                    throw new Exception("You cannot edit taxe if you are not the leader");

                fleettaxe.Name = FleetTaxeName;
                fleettaxe.Taxe = (float)FleetTaxeTaxe;
                fleettaxe.CharacterId = FleetTaxeCharacterId;
                DbContext.SaveChanges();

                return RedirectToAction("Details", new { FleetId = fleettaxe.FleetId });
            }
            catch (Exception ex)
            {
                @ViewBag.Message = "AddTaxeLogistic Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }
        public IActionResult TaxeDelete(int FleetTaxeId)
        {

            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("you shaul not pass!!! \n without being authenticated ^^");

                Fleet fleet;

                Fleettaxes fleettaxe = DbContext.Fleettaxes.Include("Fleet").FirstOrDefault(x => x.Id == FleetTaxeId);
                if (fleettaxe == null)
                    throw new Exception("Fleet taxe :" + FleetTaxeId + " not exist.");

                if (!sessionUtility.AllCharacter.ContainsKey(fleettaxe.Fleet.CharacterId))
                    throw new Exception("You cannot delete taxe if you are not the leader");

                fleet = fleettaxe.Fleet;

                DbContext.Fleettaxes.Remove(fleettaxe);
                DbContext.SaveChanges();

                return RedirectToAction("Details", new { FleetId = fleet.Id });
            }
            catch (Exception ex)
            {
                @ViewBag.Message = "Delete Fleets \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }


    }
}

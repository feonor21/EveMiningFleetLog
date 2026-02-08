using System;
using System.Linq;
using EveMiningFleet.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EveMiningFleet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

            ViewBag.Title = "Accueil";
            ViewBag.Index = true;
            ViewBag.Description = "This website was created by minors for minors :) It should simplify your management of mining fleets and their accounting ^^";
            return View();
        }
        public IActionResult CCPCopyright()
        {
            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

            ViewBag.Title = "CCPCopyright";
            ViewBag.Index = true;
            return View("CCPCopyright");
        }
        public IActionResult RoadMap()
        {
            _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

            ViewBag.Title = "RoadMap";
            ViewBag.Index = true;
            return View("RoadMap");
        }

        public IActionResult Profil()
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("You must authentified for access to profil");

                ViewBag.Title = "My Profil";
                ViewBag.Index = false;
                return View("Profil");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Access to profil :  \n" + ex.Message;
                return View("ErrorGlobal");
            }
        }
        public IActionResult ProfilHistory(int? Type)
        {
            try
            {
                _SessionUtility sessionUtility = _SessionUtility.DecryptSession(HttpContext.Session.Get(_Constante._SessionUtility));

                if (sessionUtility.MainCharacterID == 0)
                    throw new Exception("You must authentified for access to history");


                Models.ViewModel.HistoryModel returnHystory = new Models.ViewModel.HistoryModel();

                using (EveMiningFleetContext DbContext = new EveMiningFleetContext())
                {
                    bool create;

                    switch (Type)
                    {
                        case 1:
                            foreach (var cursor in sessionUtility.AllCoorp)
                            {
                                foreach (var item in DbContext.Fleets.Include("Character").Include("Corporation").Include("Alliance").Where(x => x.CorporationId == cursor.Key && x.ViewRight == 1))
                                {
                                    create = true;
                                    foreach (var list in returnHystory.listFleet)
                                    {
                                        if (list.fleet.Id == item.Id)
                                            create = false;
                                    }
                                    if (create)
                                        returnHystory.listFleet.Add(new Models.ViewModel.FleetHistoric() { fleet = item });

                                }
                            }
                            break;
                        case 2:
                            foreach (var cursor in sessionUtility.AllAlliance)
                            {
                                foreach (var item in DbContext.Fleets.Include("Character").Include("Corporation").Include("Alliance").Where(x => x.AllianceId != 0 && x.AllianceId == cursor.Key && x.ViewRight == 2))
                                {
                                    create = true;
                                    foreach (var list in returnHystory.listFleet)
                                    {
                                        if (list.fleet.Id == item.Id)
                                            create = false;
                                    }
                                    if (create)
                                        returnHystory.listFleet.Add(new Models.ViewModel.FleetHistoric() { fleet = item });
                                }
                            }
                            break;
                        default:
                            foreach (var cursor in sessionUtility.AllCharacter)
                            {
                                foreach (var item in DbContext.Fleets.Include("Character").Include("Corporation").Include("Alliance").Include("Fleetcharacters").Where(x => x.Fleetcharacters.Any(y => y.CharacterId == cursor.Key)))
                                {
                                    create = true;
                                    foreach (var list in returnHystory.listFleet)
                                    {
                                        if (list.fleet.Id == item.Id)
                                            create = false;
                                    }
                                    if (create)
                                        returnHystory.listFleet.Add(new Models.ViewModel.FleetHistoric() { fleet = item });
                                }
                            }
                            break;
                    }

                }

                return PartialView("ProfilHistoryArray", returnHystory);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Access to history :  \n" + ex.Message;
                return PartialView("ErrorGlobalPartial");
            }
        }
        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();//remove session
            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;


namespace EveMiningFleet.Controllers
{
    public class BaseController : Controller
    {
        protected EveMiningFleet.Entities.EveMiningFleetContext DbContext => (EveMiningFleet.Entities.EveMiningFleetContext)HttpContext.RequestServices.GetService(typeof(EveMiningFleet.Entities.EveMiningFleetContext));

    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EveMiningFleet.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IWebHostEnvironment _env;

        public MessagesController(ILogger<MessagesController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult errordefault()
        {
            @ViewBag.Message = "";
            @ViewBag.Title = "Error";
            return View("ErrorGlobal");
        }
        public IActionResult error403()
        {
            @ViewBag.Message = "error 403";
            @ViewBag.Title = "403";
            return View("ErrorGlobal");
        }
        public IActionResult error404()
        {
            @ViewBag.Message = "error 404 Page not found";
            @ViewBag.Title = "404 - Page Not Found";
            return View("ErrorGlobal");
        }
        public IActionResult error400()
        {
            @ViewBag.Message = "error 400 bad Request";
            @ViewBag.Title = "400";
            return View("ErrorGlobal");
        }
    }
}
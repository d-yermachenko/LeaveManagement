using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using System.Dynamic;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace LeaveManagement.Controllers {
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Help() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() //=> Problem();
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static void DisplayProblem(ILogger logger, Controller controller, string errorTitle, string errorMessage, Exception exception = null) {
            if (exception != null)
                logger?.LogError(exception, errorMessage);
            else
                logger?.LogError(errorMessage);
            controller.ModelState.AddModelError(errorTitle, errorMessage);
            controller.ViewBag.ErrorTitle = errorTitle;
            controller.ViewBag.ErrorMessage = errorMessage;
        }

        public IActionResult SwitchCulture(string cultureCode, string refController = "", string refAction ="", string refId = "") {

            ///Sorry for this code, it not the best approach, but take the route from referer is too tricky, replace the culture in the referer is too messy, 
            ///and or resend to the index too frustrating to the user
            if (GlobalizationStartup.SupportedCultures.Any(cl => cl.Name.Equals(cultureCode))) {
                dynamic routeObject = new ExpandoObject();
                routeObject.culture = cultureCode;
                if (!String.IsNullOrWhiteSpace(refController))
                    routeObject.controller = refController;
                if (!String.IsNullOrWhiteSpace(refAction))
                    routeObject.action = refAction;
                if (!String.IsNullOrWhiteSpace(refId))
                    routeObject.id = refId;
                return RedirectToRoute(routeObject);
            }
            else {
                string refererUrl = HttpContext.Request.Headers["Referer"];
                return Redirect(refererUrl);
            }
            
        }

    }
}

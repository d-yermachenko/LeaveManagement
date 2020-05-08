using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LeaveManagement.Models;

namespace LeaveManagement.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => Problem();
        /*{
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/

        public static ActionResult RedirectToError(ILogger logger, Controller controller, ErrorViewModel errorViewModel, Exception exception = null) {
            if (exception != null)
                logger.LogError(exception, errorViewModel.ErrorMessage);
            else
                logger.LogError(errorViewModel.ErrorMessage);
            return controller.View("Error", errorViewModel);
        }

        public static ActionResult RedirectToError(ILogger logger, Controller controller, string errorTitle, string errorMessage,
            object errorSource = null,
            string returnButtonUrl = null,
            string returnButtonText = null,
            Exception exception = null) {
            ErrorViewModel errorViewModel = new ErrorViewModel() {
                RequestId = controller.Request.Path,
                ErrorMessage = errorMessage,
                ErrorTitle = errorTitle
            };
            if (!String.IsNullOrWhiteSpace(returnButtonUrl)) {
                errorViewModel.StartUrl = returnButtonUrl;
                if (!String.IsNullOrWhiteSpace(returnButtonText))
                    errorViewModel.StartText = returnButtonText;
            }
            return RedirectToError(logger, controller, errorViewModel);

        }

        public static void DisplayProblem(ILogger logger, Controller controller, string errorTitle, string errorMessage, Exception exception = null) {
            if (exception != null)
                logger.LogError(exception, errorMessage);
            else
                logger.LogError(errorMessage);
            controller.ModelState.AddModelError(errorTitle, errorMessage);
            controller.ViewBag.ErrorTitle = errorTitle;
            controller.ViewBag.ErrorMessage = errorMessage;
        }
    }
}

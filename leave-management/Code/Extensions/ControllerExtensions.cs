using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LeaveManagement.Extensions {
    public static class ControllerExtensions {


        public static IActionResult DisplayView(this Controller controller, string viewName, UnauthorizedResult result) {
            return controller.View("Status401", result);
        }

        public static IActionResult DisplayView(this Controller controller, string viewName, NotFoundResult result) {
            return controller.View("Status404", result);
        }

        public static IActionResult DisplayView(this Controller controller, string viewName, RedirectResult result) {
            return controller.Redirect(result.Url);
        }

        public static IActionResult DisplayView(this Controller controller, string viewName, RedirectToActionResult result) {
            return controller.RedirectToAction(result.ActionName, result.ControllerName, result.Fragment, );
        }

        public static IActionResult DisplayView(this Controller controller, string viewName, BadRequestResult result) {
            return controller.View(viewName, result);
        }
    }
}

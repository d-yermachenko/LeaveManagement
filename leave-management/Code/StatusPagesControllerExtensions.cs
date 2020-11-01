using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code {

    public class MessageViewModel {
        public string Header { get; set; }

        public string Message { get; set; }

        public bool IsDefault { get; set; }

    }

    public static class StatusPagesControllerExtensions {
        public const string ForbidView = "Forbid";

        public static ViewResult ForbidDisplay(this Controller controller,
            string header = "Operation forbidden", string message = "The operation you trying to execute is forbidden for your actual status") {
            MessageViewModel viewModel = new MessageViewModel() {
                Header = header,
                Message = message
            };
            return controller.View(ForbidView, viewModel);
        }

        public static ViewResult NotFoundDisplay(this Controller controller,
            string header = "Page not found", string message = "The page or object is not found") {
            MessageViewModel viewModel = new MessageViewModel() {
                Header = header,
                Message = message
            };
            return controller.View(ForbidView, viewModel);
        }

        public static ViewResult BadRequestDisplay(this Controller controller,
    string header = "Page not found", string message = "The page or object is not found") {
            MessageViewModel viewModel = new MessageViewModel() {
                Header = header,
                Message = message
            };
            return controller.View(ForbidView, viewModel);
        }


    }
}

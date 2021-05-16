using LeaveManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Filters {
    public class HttpErrorFilter : IAsyncActionFilter {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var resultContext = await next();
            ErrorViewModel errorModel = GetErrorViewModel(resultContext.Result as dynamic);
            if (errorModel == null)
                return;
            resultContext.Result = GetErrorView(errorModel, (resultContext.Controller as Controller)?.ViewData);
        }


        private ErrorViewModel GetErrorViewModel(ObjectResult result) {
            return new ErrorViewModel() {
                ErrorCode = result.StatusCode?? StatusCodes.Status500InternalServerError,
                ErrorMessage = result.Value.ToString()
            };
        }

        private ErrorViewModel GetErrorViewModel(NotFoundResult result) {
            return new ErrorViewModel() {
                ErrorCode =  StatusCodes.Status404NotFound
            };
        }


        private ErrorViewModel GetErrorViewModel(ViewResult result) {
            return null;
        }

        public IActionResult GetErrorView(ErrorViewModel errorModel, ViewDataDictionary viewDataDictionary) {
            if (errorModel == null)
                return GetUnpredictedResult(errorModel, viewDataDictionary);
            return errorModel?.ErrorCode switch {
                StatusCodes.Status404NotFound => GetNotFoundView(errorModel, viewDataDictionary),
                StatusCodes.Status403Forbidden => GetForbiddenView(errorModel, viewDataDictionary),
                StatusCodes.Status500InternalServerError => GetInternalServerErrorView(errorModel, viewDataDictionary),
                StatusCodes.Status400BadRequest => GetInternalServerErrorView(errorModel, viewDataDictionary),
                StatusCodes.Status401Unauthorized => GetNotAuthorizedResult(errorModel, viewDataDictionary),
                StatusCodes.Status501NotImplemented => GetNotImplementedResult(errorModel, viewDataDictionary),
                _ => GetUnpredictedResult(errorModel, viewDataDictionary)

            };
        }

        public IActionResult GetNotFoundView(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "NotFoundError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

        public IActionResult GetForbiddenView(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "ForbiddenError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

        public IActionResult GetInternalServerErrorView(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "InternalServerError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

        public IActionResult GetBadRequestView(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "BadRequestError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

        public IActionResult GetNotAuthorizedResult(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new RedirectToRouteResult(new { });
        }

        public IActionResult GetNotImplementedResult(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "InternalServerError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

        public IActionResult GetUnpredictedResult(ErrorViewModel errorViewModel, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "SomeUnknownError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = errorViewModel
                }
            };
        }

    }
}

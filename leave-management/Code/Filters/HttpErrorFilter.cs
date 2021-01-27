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
            if (!(resultContext.Result is ObjectResult))
                return;
            ObjectResult objectResult = (ObjectResult)resultContext.Result;
            if (objectResult.StatusCode == null)
                return;
            int statusCode = (int)objectResult.StatusCode;
            switch (statusCode) {
                case StatusCodes.Status404NotFound:
                    resultContext.Result = GetNotFoundView(objectResult.Value.ToString(), ((Controller)resultContext.Controller).ViewData);
                    break;
                case StatusCodes.Status403Forbidden:
                    resultContext.Result = GetForbiddenView(objectResult.Value.ToString(), ((Controller)resultContext.Controller).ViewData);
                    break;
                case StatusCodes.Status500InternalServerError:
                    resultContext.Result = GetInternalServerErrorView(objectResult.Value.ToString(), ((Controller)resultContext.Controller).ViewData);
                    break;
            }
        }

        public IActionResult GetNotFoundView(string message, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "NotFoundError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = new ErrorViewModel() {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = message
                    }
                }
            };
        }

        public IActionResult GetForbiddenView(string message, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "ForbiddenError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = new ErrorViewModel() {
                        ErrorCode = StatusCodes.Status403Forbidden,
                        ErrorMessage = message

                    }
                }
            };
        }

        public IActionResult GetInternalServerErrorView(string message, ViewDataDictionary viewDataDictionary) {
            return new ViewResult() {
                ViewName = "InternalServerError",
                ViewData = new ViewDataDictionary(viewDataDictionary) {
                    Model = new ErrorViewModel() {
                        ErrorCode = StatusCodes.Status403Forbidden,
                        ErrorMessage = message

                    }
                }
            };
        }
    }
}

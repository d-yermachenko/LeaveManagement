using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code.CustomLocalization {

    public class LanguageRouteConstraint : IRouteConstraint {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {

            if (!httpContext.Request.RouteValues.ContainsKey("culture"))
                return false;

            var culture = values["culture"].ToString();
            return new string[] { "en", "en-US", "ru", "fr", "fr-FR" }.Contains(culture);
        }
    }
}

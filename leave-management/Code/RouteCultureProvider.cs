using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code {
    public class RouteCultureProvider : IRequestCultureProvider {
        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext) {
            return Task.FromResult(new ProviderCultureResult(new Microsoft.Extensions.Primitives.StringSegment(
                    httpContext.GetRouteValue("culture")?.ToString()??String.Empty
                )));
        }
    }

    public class LanguageRouteConstraint : IRouteConstraint {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {

            if (!values.ContainsKey("culture"))
                return false;

            var culture = values["culture"].ToString();
            return new string[] { "en", "en-US", "ru", "fr", "fr-FR" }.Contains(culture);
        }
    }
}

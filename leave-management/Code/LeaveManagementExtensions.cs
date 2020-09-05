using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace LeaveManagement {
    public static class LeaveManagementExtensions {
        public static string FormatEmployeeSNT(this Employee employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }

        public static string FormatEmployeeSNT(this EmployeePresentationDefaultViewModel employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }


        public static async Task<bool> IsMemberOfOneAsync<T>(this UserManager<T> userManager, T user, UserRoles roles) where T : class {
            if (user == null)
                return false;
            UserRoles userRoles = ToUserRoles(await userManager.GetRolesAsync(user));
            return (userRoles & roles) > 0;
        }

        public static async Task<bool> IsPrivelegedUser<T>(this UserManager<T> userManager, System.Security.Claims.ClaimsPrincipal user) where T : class {
            T appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
                return false;
            return await userManager.IsPrivelegedUser<T>(appUser);
        }

        public static async Task<bool> IsPrivelegedUser<T>(this UserManager<T> userManager, T user) where T : class {
            UserRoles privelegedRoles = UserRoles.HRManager | UserRoles.Administrator | UserRoles.LocalAdministrator;
            var rolesStrings = await userManager.GetRolesAsync(user);
            var roles = ToUserRoles(rolesStrings );
            return (roles & privelegedRoles) > 0;
        }

        public static async Task<UserRoles> GetUserRolesAsync<T>(this UserManager<T> userManager, T user) where T: class {
            var roles = await userManager.GetRolesAsync(user);
            return ToUserRoles(roles);
        }

        public static async Task<UserRoles> GetUserRolesAsync<T>(this UserManager<T> userManager, System.Security.Claims.ClaimsPrincipal user) where T : class {
            return await userManager.GetUserRolesAsync(await userManager.GetUserAsync(user));
        }

        #region Convertion between roles and strings
        public static string[] FromUserRoles(UserRoles userRoles) {
            List<string> rolesNames = new List<string>();
            var userRolesValues = Enum.GetValues(typeof(UserRoles));
            foreach (var userRoleValue in userRolesValues) {
                UserRoles userRole = (UserRoles)userRoleValue;
                if ((userRole & userRoles) > 0)
                    rolesNames.Add(Enum.GetName(typeof(UserRoles), userRole));
            }
            return rolesNames.ToArray();
        }

        public static UserRoles ToUserRoles(IEnumerable<string> rolesNames) {
            UserRoles result = UserRoles.None;
            foreach(var roleName in rolesNames) {
                if (Enum.TryParse<UserRoles>(roleName, out UserRoles roleValue)) {
                    result |= roleValue;
                }
            }
            return result;
        }
        #endregion


    }

    public static class HtmlHelpersExtensions {

        private static IStringLocalizerFactory _LocalizerFactory;

        public static void RegisterLocalizer(IStringLocalizerFactory localizerFactory) {
            _LocalizerFactory = localizerFactory;
        }

        public static string DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string description = displayAttribute?.Description ?? memberExpression.Member?.Name;
            description = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[description]) ?? description;
            return description;
        }

        public static string ShortNameFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string shortName = displayAttribute?.ShortName ?? memberExpression.Member?.Name;
            shortName = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[shortName]) ?? shortName;
            return shortName;
        }

        public static string PromptFor<TModel, TValue>(this IHtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression) {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            var displayAttribute = (DisplayAttribute)memberExpression.Member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            string prompt = displayAttribute?.Prompt ?? memberExpression.Member?.Name;
            prompt = (_LocalizerFactory?.Create(memberExpression.Expression.Type)[prompt]) ?? prompt;
            return prompt;
        }
    }

    
}
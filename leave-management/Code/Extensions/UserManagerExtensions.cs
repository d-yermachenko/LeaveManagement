using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LeaveManagement {


    public static class UserManagerExtensions {
        public static async Task<bool> IsMemberOfOneAsync<T>(this UserManager<T> userManager, System.Security.Claims.ClaimsPrincipal user, UserRoles roles) where T : class {
            T connectedUser = await userManager.GetUserAsync(user);
            return await IsMemberOfOneAsync(userManager, connectedUser, roles);
        }


        public static async Task<bool> IsMemberOfOneAsync<T>(this UserManager<T> userManager, T user, UserRoles roles) where T : class {
            if (user == null)
                return false;
            UserRoles userRoles = ToUserRoles(await userManager.GetRolesAsync(user));
            return (userRoles & roles) > 0;
        }

        public static async Task<bool> IsCompanyPrivelegedUser<T>(this UserManager<T> userManager, System.Security.Claims.ClaimsPrincipal user) where T : class {
            T appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
                return false;
            return await userManager.IsCompanyPrivelegedUser<T>(appUser);
        }

        public static async Task<bool> IsCompanyPrivelegedUser<T>(this UserManager<T> userManager, T user) where T : class {
            if (user == null)
                return false;
            var rolesStrings = await userManager.GetRolesAsync(user);
            var roles = ToUserRoles(rolesStrings );
            return await userManager.IsCompanyPrivelegedUser(roles);
        }

        public static async Task<bool> IsCompanyPrivelegedUser<T>(this UserManager<T> userManager, UserRoles roles) where T : class {
            UserRoles privelegedRoles = UserRoles.HRManager | UserRoles.CompanyAdministrator;
            return await Task.FromResult((roles & privelegedRoles) > 0);
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
            if(rolesNames == null)
                return UserRoles.None;
            UserRoles result = UserRoles.None;
            foreach (var roleName in rolesNames) {
                if (Enum.TryParse<UserRoles>(roleName, out UserRoles roleValue)) {
                    result |= roleValue;
                }
            }
            return result;
        }
        #endregion
    }

    
}
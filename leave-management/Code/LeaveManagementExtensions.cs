using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Data;

using System.Threading.Tasks.Dataflow;

namespace LeaveManagement {
    public static class LeaveManagementExtensions {
        public static string FormatEmployeeSNT(this Employee employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }

        public static string FormatEmployeeSNT(this EmployeePresentationDefaultViewModel employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }

        public static async Task<bool> IsUserHasOneRoleOfAsync<T>(this UserManager<T> userManager, T user, params string[] roles) where T: class {
            IList<string> userRoles = await userManager.GetRolesAsync(user);
            return userRoles.Any(role => roles.Contains(role));
        }


    }
}
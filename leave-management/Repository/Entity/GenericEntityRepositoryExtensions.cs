using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Repository.Entity {
    public static class GenericEntityRepositoryExtensions {
        public static async Task<bool> RegisterEmployeeAsync(this IRepository<Employee> repository, UserManager<IdentityUser> userManager, Employee entity, string password) {
            var identityResult = await userManager.CreateAsync(entity);
            if (!identityResult.Succeeded)
                throw new OperationFailedException("Failed to create new employee");
            IdentityUser newUser = await userManager.FindByIdAsync(entity.Id);
            bool hasUser = newUser != null;
            if (!hasUser)
                return false;
            var attachedEmployee = await repository.FindAsync(x => x.Id.Equals(newUser.Id));
            if (attachedEmployee == null)
                return false;
            bool result = (await userManager.AddPasswordAsync(entity, password)).Succeeded;
            return result;
        }

        public static async Task<Employee> GetEmployeeAsync(this IRepository<Employee> repository, System.Security.Claims.ClaimsPrincipal user) {
            return await repository.FindAsync(x => x.UserName == user.Identity.Name);
        }

        public static async Task<bool> SetEmployeesRoles(this IRepository<Employee> repository, UserManager<IdentityUser> userManager, Employee employee, UserRoles roles) {
            bool hasEmployee = (await repository.FindAsync(x => x == employee)) != null;
            if (!hasEmployee)
                return false;
            ICollection<string> oldUserRoles = await userManager.GetRolesAsync(employee);
            bool updateResult = true;
            UserRoles newRoles = roles;
            UserRoles oldRoles = await userManager.GetUserRolesAsync(employee);
            var addedRoles = UserManagerExtensions.FromUserRoles((oldRoles ^ newRoles) & newRoles);
            var removedRoles = UserManagerExtensions.FromUserRoles((oldRoles ^ newRoles) & oldRoles);
            updateResult &= (await userManager.AddToRolesAsync(employee, addedRoles)).Succeeded;
            updateResult &= (await userManager.RemoveFromRolesAsync(employee, removedRoles)).Succeeded;
            return updateResult;
        }
    }
}

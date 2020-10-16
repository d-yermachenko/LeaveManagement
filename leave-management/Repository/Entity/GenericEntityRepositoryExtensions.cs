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
            bool hasEmployee = (await repository.FindAsync(x => x.Id.Equals(newUser.Id))) != null;
            return hasEmployee;
        }

        public static async Task<Employee> GetEmployeeAsync(this IRepository<Employee> repository, System.Security.Claims.ClaimsPrincipal user) {
            return await repository.FindAsync(x => x.UserName == user.Identity.Name);
        }

    }
}

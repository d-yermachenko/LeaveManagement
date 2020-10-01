using LeaveManagement.Code;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LeaveManagement {
    public static class SeedData {
        public const string DefaultAdminUserName = "admin@localhost.com";
        public const string DefaultAdminPassword = "P@ssw0rd";

        public async static Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger) {
            try { 
                await SeedRoles(roleManager);
                await SeedUsers(userManager);
            }
            catch(AggregateException e) {
                foreach (var innerException in e.Flatten().InnerExceptions) {
                    logger?.LogError(innerException, "Error in seeding");
                }
            }
        }

        private async static Task SeedUsers(UserManager<IdentityUser> userManager) {
            var possibleAdminUser = await userManager.FindByNameAsync(DefaultAdminUserName);
            bool success = true;
            if (possibleAdminUser == null) {
                IdentityUser adminUser = new IdentityUser() {
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminUserName
                };
                var newBornAdmin = (await userManager.CreateAsync(adminUser, DefaultAdminPassword));
                success &= newBornAdmin.Succeeded;
                if (success)
                    possibleAdminUser = await userManager.FindByNameAsync(DefaultAdminUserName);
            }
            if (success && !await userManager.IsInRoleAsync(possibleAdminUser, UserRoles.AppAdministrator.ToString()))
                success &= (await userManager.AddToRoleAsync(possibleAdminUser, UserRoles.AppAdministrator.ToString())).Succeeded;

            if (success && await userManager.IsInRoleAsync(possibleAdminUser, UserRoles.Employee.ToString()))
                success &= (await userManager.RemoveFromRoleAsync(possibleAdminUser, UserRoles.Employee.ToString())).Succeeded;

            if (!success)
                throw new OperationFailedException();

        }

        private async static Task SeedRoles(RoleManager<IdentityRole> roleManager) {
            string[] defaultRoles = Enum.GetNames(typeof(UserRoles));
            await Task.Run(async () =>  {
                foreach (var role in defaultRoles) {
                    if (!await roleManager.RoleExistsAsync(role)) {
                        await roleManager.CreateAsync(new IdentityRole() { Name = role });
                    }
                }
            });
        }

    }
}

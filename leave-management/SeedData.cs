using LeaveManagement.Code;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LeaveManagement {
    public static class SeedData {
        private const string DefaultAdminUserName = "admin@localhost.com";
        private const string DefaultAdminPassword = "P@ssw0rd";

        public const string AdministratorRole = "Administrator";
        public const string EmployeeRole = "Employee";
        public const string HRStaffRole = "HRStaff";


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
            if (possibleAdminUser == null) {
                IdentityUser adminUser = new IdentityUser() {
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminUserName
                };
                var newBornAdmin = (await userManager.CreateAsync(adminUser, DefaultAdminPassword));
                if(newBornAdmin.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, AdministratorRole);

            }


        }

        private async static Task SeedRoles(RoleManager<IdentityRole> roleManager) {
            string[] defaultRoles = new string[] { 
                AdministratorRole, 
                EmployeeRole, 
                HRStaffRole };
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

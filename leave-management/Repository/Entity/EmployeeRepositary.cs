using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Repository.Entity {
    public class EmployeeRepository : IEmployeeRepositoryAsync {

        private ApplicationDbContext _ApplicationDbContext;

        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;

        private readonly UserManager<IdentityUser> _UserManager;

        private readonly SignInManager<IdentityUser> _SignInManager;

        public EmployeeRepository(ILeaveManagementCustomLocalizerFactory localizerFactory,
            ApplicationDbContext applicationDbContext,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) {
            _ApplicationDbContext = applicationDbContext;
            _LocalizerFactory = localizerFactory;
            _UserManager = userManager;
            _SignInManager = signInManager;
        }
        public async Task<bool> CreateAsync(Employee entity) {
            var result = await _UserManager.CreateAsync(entity);
            return result.Succeeded;
        }

        public async Task<bool> DeleteAsync(Employee entity) => await _UserManager.DeleteAsync(entity).ContinueWith((identityResult) => identityResult.Result.Succeeded);

        public async Task<ICollection<Employee>> FindAllAsync() => await Task.Run(() => (ICollection<Employee>)_ApplicationDbContext.Employees.ToList());

        public async Task<Employee> FindByIdAsync(string id) {
            var hasEmployee = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(id));
            var hasUser = await _ApplicationDbContext.Users.AnyAsync(x => x.Id.Equals(id));
            if (hasEmployee)
                return await _ApplicationDbContext.Employees.FindAsync(id);
            else if(hasUser) {
                var user = await _ApplicationDbContext.Users.FindAsync(id);
                var employee = new Employee() {
                    AccessFailedCount = user.AccessFailedCount,
                    ConcurrencyStamp = user.ConcurrencyStamp,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    Id = user.Id,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    NormalizedEmail = user.NormalizedEmail,
                    NormalizedUserName = user.NormalizedUserName,
                    PasswordHash = user.PasswordHash,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    SecurityStamp = user.SecurityStamp,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    UserName = user.UserName
                };
                _ApplicationDbContext.Remove(user);
                _ApplicationDbContext.Add(employee);
                var changes = await _ApplicationDbContext.SaveChangesAsync();
                if(changes > 0)
                    return await _ApplicationDbContext.Employees.FindAsync(id);
                else
                    throw new KeyNotFoundException(_LocalizerFactory.MiscelanousLocalizer["User not found"]);
            }

            return await Task.Run(() => _ApplicationDbContext.Employees.First(x => x.Id.Equals(id)));
        }

        public Task<bool> SaveAsync() => _ApplicationDbContext.SaveChangesAsync().ContinueWith((saveResult) => saveResult.Result > 0);

        public async Task<bool> UpdateAsync(Employee entity) {
            var userData = await FindByIdAsync(entity.Id);
            if (userData is null)
                throw new ArgumentOutOfRangeException(_LocalizerFactory.MiscelanousLocalizer["User you provided was not yet created"]);
            #region Modification
            userData.Title = entity.Title;

            userData.FirstName = entity.FirstName;

            userData.LastName = entity.LastName;

            userData.DisplayName = entity.DisplayName;

            userData.PhoneNumber = entity.PhoneNumber;

            userData.TaxRate = entity.TaxRate;
            if (!userData?.DateOfBirth.Equals(entity.DateOfBirth) ?? true)
                userData.DateOfBirth = entity.DateOfBirth;
            if (!userData?.EmploymentDate.Equals(entity.EmploymentDate) ?? true)
                userData.EmploymentDate = entity.EmploymentDate;

            #endregion

            _ApplicationDbContext.Update(userData);
            await _SignInManager.RefreshSignInAsync(userData);
            return await SaveAsync();



        }
    }
}

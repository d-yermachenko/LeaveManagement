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

        public async Task<Employee> FindByIdAsync(string id) => await Task.Run(() => _ApplicationDbContext.Employees.First(x => x.Id.Equals(id)));

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

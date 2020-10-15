using AutoMapper;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using LeaveManagement.PasswordGenerator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LeaveManagement.Repository.Entity {
    public class EmployeeRepository : IEmployeeRepositoryAsync {

        private ApplicationDbContext _ApplicationDbContext;

        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;

        private readonly UserManager<IdentityUser> _UserManager;

        private readonly SignInManager<IdentityUser> _SignInManager;

        private readonly IMapper _Mapper;

        private ILogger<EmployeeRepository> _Logger;

        private IEmailSender _EmailSender;

        private readonly IStringLocalizer _EmployeeLocalizer;

        private readonly IPasswordGenerator _PasswordGenerator;

        public EmployeeRepository(ILeaveManagementCustomLocalizerFactory localizerFactory,
            ApplicationDbContext applicationDbContext,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMapper mapper,
            ILogger<EmployeeRepository> logger,
            IEmailSender emailSender,
            IPasswordGenerator passwordGenerator) {
            _ApplicationDbContext = applicationDbContext;
            _LocalizerFactory = localizerFactory;
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Logger = logger;
            _Mapper = mapper;
            _EmailSender = emailSender;
            _EmployeeLocalizer = _LocalizerFactory.Create(this.GetType());
            _PasswordGenerator = passwordGenerator;
        }

        public async Task<bool> RegisterEmployeeAsync(Employee entity, string password) {
            if (await CreateAsync(entity)) {
                var result = await _UserManager.AddPasswordAsync(entity, password);
                if (result.Succeeded) {
                    var updatedUser = await _UserManager.FindByNameAsync(entity.UserName);
                }
                return result.Succeeded;
            }
            else
                return false;
        }


        public async Task<bool> CreateAsync(Employee entity) {
            var result = await _UserManager.CreateAsync(entity);
            if (!result.Succeeded)
                throw new InvalidOperationException(result.ToString());
            IdentityUser newUser = await _UserManager.FindByIdAsync(entity.Id);
            bool hasUser = newUser != null;
            if (!hasUser)
                return false;
            bool hasEmployee = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(newUser.Id));
            return hasEmployee;
        }

        public async Task<bool> DeleteAsync(Employee entity) => await _UserManager.DeleteAsync(entity).ContinueWith((identityResult) => identityResult.Result.Succeeded);

        public async Task<ICollection<Employee>> FindAllAsync() => await Task.Run(() =>
        (ICollection<Employee>)_ApplicationDbContext.Employees.Include(c=>c.Company).ToList());

        public async Task<Employee> FindByIdAsync(string id) {
            var hasEmployee = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(id));
            var hasUser = await _ApplicationDbContext.Users.AnyAsync(x => x.Id.Equals(id));
            if (hasEmployee)
                return await _ApplicationDbContext.Employees.Include(x => x.Company).FirstAsync(x => x.Id.Equals(id));
            else
                return null;
            /*else if (hasUser) {
                var user = await _ApplicationDbContext.Users.FindAsync(id);
                var employee = _Mapper.Map<Employee>(user);
                _ApplicationDbContext.Remove(user);
                _ApplicationDbContext.Add(employee);
                var changes = await _ApplicationDbContext.SaveChangesAsync();
                if (changes > 0)
                    return await _ApplicationDbContext.Employees.FindAsync(id);
                else
                    throw new KeyNotFoundException(_EmployeeLocalizer["User not found"]);
            }

            return await Task.Run(() => _ApplicationDbContext.Employees.First(x => x.Id.Equals(id)));*/
        }

        public Task<bool> SaveAsync() => _ApplicationDbContext.SaveChangesAsync().ContinueWith((saveResult) => saveResult.Result > 0);

        public async Task<bool> UpdateAsync(Employee entity) {
            var userExists = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(entity.Id));
            if (!userExists)
                throw new ArgumentOutOfRangeException(_EmployeeLocalizer["User you provided was not yet created"]);
            _ApplicationDbContext.Update(entity);
            return await SaveAsync();

        }

        public async Task<ICollection<Employee>> WhereAsync(Func<Employee, bool> predicate) {
            return await Task.Run(() => _ApplicationDbContext.Employees.
            Include(c=>c.Company).Where(predicate).ToList());
        }

        public async Task<Employee> GetEmployeeAsync(ClaimsPrincipal user) {
            var employeeUser = await _UserManager.GetUserAsync(user);
            return await FindByIdAsync(employeeUser.Id);
        }
    }
}

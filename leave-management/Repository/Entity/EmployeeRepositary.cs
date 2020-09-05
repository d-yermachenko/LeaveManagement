using AutoMapper;
using KellermanSoftware.CompareNetObjects;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
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

        public EmployeeRepository(ILeaveManagementCustomLocalizerFactory localizerFactory,
            ApplicationDbContext applicationDbContext,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMapper mapper,
            ILogger<EmployeeRepository> logger,
            IEmailSender emailSender) {
            _ApplicationDbContext = applicationDbContext;
            _LocalizerFactory = localizerFactory;
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Logger = logger;
            _Mapper = mapper;
            _EmailSender = emailSender;
            _EmployeeLocalizer = _LocalizerFactory.Create(this.GetType());
        }

        public async Task<bool> RegisterEmployeeAsync(Employee entity, string password) {
            entity.Id = (new IdentityUser() { UserName = entity.UserName, Email = entity.Email }).Id;
            var result = await _UserManager.CreateAsync(entity, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(result.ToString());
            IdentityUser newUser = await _UserManager.FindByIdAsync(entity.Id);
            bool hasUser = newUser != null;
            if (!hasUser)
                return false;
            bool hasEmployee = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(newUser.Id));
            return hasEmployee;
        }

        public async Task<bool> CreateAsync(Employee entity) {
            string password = $"{entity.FormatEmployeeSNT()}@{DateTime.Now.Year}-{DateTime.Now.Month}";
            bool registrationResult = await RegisterEmployeeAsync(entity, password);
            LocalizedHtmlString mail = _LocalizerFactory.HtmlIdentityLocalizer["Your password : {0}", password];
            StringBuilder mailBodyBuilder = new StringBuilder();
            using (StringWriter writer = new StringWriter(mailBodyBuilder)) {
                mail.WriteTo(writer, HtmlEncoder.Create(new TextEncoderSettings()));
            }
            await _EmailSender?.SendEmailAsync(entity.Email, _LocalizerFactory.StringIdentityLocalizer["Your password"], mailBodyBuilder.ToString());
            return registrationResult;
        }

        public async Task<bool> DeleteAsync(Employee entity) => await _UserManager.DeleteAsync(entity).ContinueWith((identityResult) => identityResult.Result.Succeeded);

        public async Task<ICollection<Employee>> FindAllAsync() => await Task.Run(() => (ICollection<Employee>)_ApplicationDbContext.Employees.ToList());

        public async Task<Employee> FindByIdAsync(string id) {
            var hasEmployee = await _ApplicationDbContext.Employees.AnyAsync(x => x.Id.Equals(id));
            var hasUser = await _ApplicationDbContext.Users.AnyAsync(x => x.Id.Equals(id));
            if (hasEmployee)
                return await _ApplicationDbContext.Employees.FindAsync(id);
            else if (hasUser) {
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

            return await Task.Run(() => _ApplicationDbContext.Employees.First(x => x.Id.Equals(id)));
        }

        public Task<bool> SaveAsync() => _ApplicationDbContext.SaveChangesAsync().ContinueWith((saveResult) => saveResult.Result > 0);

        public async Task<bool> UpdateAsync(Employee entity) {
            var userData = await FindByIdAsync(entity.Id);
            if (userData is null)
                throw new ArgumentOutOfRangeException(_EmployeeLocalizer["User you provided was not yet created"]);
            userData = _Mapper.Map<Employee>(entity);
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

            return await SaveAsync();



        }

        public async Task<ICollection<Employee>> WhereAsync(Func<Employee, bool> predicate) {
            return await Task.Run(() => _ApplicationDbContext.Employees.Where(predicate).ToList());
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using LeaveManagement.Code;

namespace LeaveManagement.Controllers {
    [Authorize]
    public class CompanyController : Controller {
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly ILogger<CompanyController> _CompanyControllerLogger;
        private readonly IStringLocalizer _MessageLocalizer;
        private readonly IMapper _Mapper;
        private readonly IEmailSender _EmailSender;

        private const string EditionViewName = "Edit";

        public CompanyController(
           ILeaveManagementUnitOfWork unitOfWork,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<CompanyController> logger,
            IStringLocalizerFactory localizerFactory,
            IMapper mapper,
            IEmailSender emailSender
            ) {
            _UnitOfWork = unitOfWork;
            _UserManager = userManager;
            _SignInManager = signInManager;
            _CompanyControllerLogger = logger;
            _MessageLocalizer = localizerFactory.Create(this.GetType());
            _Mapper = mapper;
            _EmailSender = emailSender;
        }

        // GET: CompanyController
        [HttpGet]
        public async Task<ActionResult> IndexAction(bool showDisabled = false) {
            var currentUser = await _UserManager.GetUserAsync(User);
            if (!(await _UserManager.IsMemberOfOneAsync(currentUser, UserRoles.AppAdministrator))) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                return Unauthorized(_MessageLocalizer["Your not allowed to list the companies"]);
            }
            IEnumerable<CompanyVM> companies = _Mapper.Map<List<CompanyVM>>(await _UnitOfWork.Companies.WhereAsync(
                filter: c => c.Active || showDisabled,
                order: x => x.OrderBy(c => c.CompanyName),
                includes: Array.Empty<System.Linq.Expressions.Expression<Func<Company, object>>>()));
            return View(companies);
        }

        public async Task<ActionResult> Index(bool showDisabled = false) => await IndexAction(showDisabled);

        // GET: CompanyController/Details/5
        public async Task<ActionResult> Details(int id) {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(currentUser.Id),
                includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { e=>e.Company, e=>e.Manager }) ;
            bool userAuthorizedToViewDetails = currentEmployee != null || currentEmployee?.CompanyId == id
                || (await _UserManager.IsMemberOfOneAsync(currentUser, UserRoles.AppAdministrator));
            if (!userAuthorizedToViewDetails) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
            }
            var companyData = await _UnitOfWork.Companies.FindAsync(predicate: x=>x.Id == id);
            if (companyData == null) {
                _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                return NotFound(_MessageLocalizer["Company not found"]);
            }
            CompanyVM companyVM = _Mapper.Map<CompanyVM>(companyData);
            if ((currentUserRoles & (UserRoles.AppAdministrator | UserRoles.CompanyAdministrator)) == 0)
                companyVM.CompanyProtectedComment = _MessageLocalizer["Hidden"];
            return View(companyVM);
        }

        // GET: CompanyController/Create
        public async Task<ActionResult> Create() {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            bool roleAllowsCreateCompany = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            if (!roleAllowsCreateCompany) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to create the company record");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to create the companies"]);
                return Forbid(_MessageLocalizer["Your not allowed to create the companies"]);
            }
            CompanyVM companyVM = new CompanyVM() { Id = 0, CompanyRegistrationDate = DateTime.Now, Active=true };
            ViewData["Action"] = nameof(Create);
            return View(EditionViewName, companyVM);
        }

        // POST: CompanyController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CompanyVM companyVM) {
            try {
                var currentUser = await _UserManager.GetUserAsync(User);
                UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
                bool roleAllowsCreateCompany = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
                if (!roleAllowsCreateCompany) {
                    _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to create company record");
                    ModelState.AddModelError("", _MessageLocalizer["Your not allowed to create the companies"]);
                    return Forbid(_MessageLocalizer["Your not allowed to create the companies"]);
                }
                Company company = _Mapper.Map<Company>(companyVM);
                await _UnitOfWork.Companies.CreateAsync(company);
                try {
                    if (!(await _UnitOfWork.Save())) {
                        ModelState.AddModelError("", _MessageLocalizer["Unabled to create the company due the server error"]);
                        _CompanyControllerLogger.LogError("Unabled to create the company due the server error");
                    }
                }
                catch(AggregateException ae) {
                    _CompanyControllerLogger.LogError(ae.Flatten(), ae.Message);
                    ModelState.AddModelError("", _MessageLocalizer["Unabled to create the company due the server error"]);
                }

                return RedirectToAction(nameof(Index));
            }
            catch {
                ViewData["Action"] = nameof(Create);
                return View(EditionViewName, companyVM);
            }
        }

        // GET: CompanyController/Edit/5
        public async Task<ActionResult> Edit(int id) {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(currentUser.Id));
            bool roleAllowsEditCompanyData = ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                || (((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator) && currentEmployee?.CompanyId != id);
            if (!roleAllowsEditCompanyData) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
            }
            var companyData = await _UnitOfWork.Companies.FindAsync(x=>x.Id == id);
            if (companyData == null) {
                _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                return NotFound(_MessageLocalizer["Company not found"]);
            }
            CompanyVM companyVM = _Mapper.Map<CompanyVM>(companyData);
            ViewData["Action"] = nameof(Edit);
            return View(EditionViewName, companyVM);
        }

        // POST: CompanyController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CompanyVM companyVM) {
            try {
                var currentUser = await _UserManager.GetUserAsync(User);
                UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
                var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>id.Equals(currentUser.Id));
                bool roleAllowsEditCompanyData = ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                    || (((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator) && currentEmployee?.CompanyId != id);
                if (!roleAllowsEditCompanyData) {
                    _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                    ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                    return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
                }
                var companyData = await _UnitOfWork.Companies.FindAsync(x=>x.Id==id);
                if (companyData == null) {
                    _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                    ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                    return NotFound(_MessageLocalizer["Company not found"]);
                }
                companyData = _Mapper.Map<CompanyVM, Company>(companyVM, companyData, mappingOptions=> {
                    mappingOptions.Items["Id"] = id;
                    mappingOptions.Items["Active"] = companyData.Active;
                });
                await _UnitOfWork.Companies.UpdateAsync(companyData);
                if (await _UnitOfWork.Save())
                    return RedirectToAction(nameof(Index));
                else {
                    ModelState.AddModelError("", _MessageLocalizer["Unabled to create the company due the server error"]);
                    ViewData["Action"] = nameof(Edit);
                    return View(EditionViewName, companyVM);
                }
            }
            catch (Exception e) {
                _CompanyControllerLogger.LogError(e, e.Message, Array.Empty<object>());
                ViewData["Action"] = nameof(Edit);
                return View(EditionViewName, companyVM);
            }
        }

        // GET: CompanyController/Delete/5
        public async Task<ActionResult> DisableCompany(int id) {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(currentUser.Id));
            bool roleAllowsEditCompanyData = ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                || (((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator) && currentEmployee?.CompanyId != id);
            if (!roleAllowsEditCompanyData) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
            }
            var companyData = await _UnitOfWork.Companies.FindAsync(x=>x.Id == id);
            if (companyData == null) {
                _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                return NotFound(_MessageLocalizer["Company not found"]);
            }

            bool operationResult = true;
            var employeesOfTheCompany = await _UnitOfWork.Employees.WhereAsync(emp => emp.CompanyId == id);
            StringBuilder lockedEmployees = new StringBuilder();
            foreach (var employee in employeesOfTheCompany) {
                if (!await (_UserManager.IsMemberOfOneAsync(employee, UserRoles.CompanyAdministrator))) {
                    employee.LockoutEnabled = true;
                    employee.LockoutEnd = DateTime.MaxValue;
                    operationResult &= await _UnitOfWork.Employees.UpdateAsync(employee);
                    if (operationResult)
                        lockedEmployees.Append($"{{ id:{employee.Id}, \nuserName: {employee.UserName} }}");
                    else
                        break;
                }
            }
            if (operationResult) {
                companyData.Active = false;
                operationResult &= await _UnitOfWork.Companies.UpdateAsync(companyData);
            }
            operationResult &= await _UnitOfWork.Save();
            if (!operationResult) {
                ModelState.AddModelError("", "Unabled to lock company.");
                await SendNotificationToAppAdmins($"Unable to lock company #{id}, list of the employees which was locked:\n {lockedEmployees}",
                    subject: $"Company {companyData.CompanyName} deactivating emergency");
                return RedirectToAction(nameof(Details), new { id });
            }
            else
                return RedirectToAction(nameof(Index), new { showDisabled = true });


        }

        public async Task<ActionResult> EnableCompany(int id) {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id==currentUser.Id);
            bool roleAllowsEditCompanyData = ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                || (((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator) && currentEmployee?.CompanyId != id);
            if (!roleAllowsEditCompanyData) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
            }
            var companyData = await _UnitOfWork.Companies.FindAsync(x=>x.Id==id);
            if (companyData == null) {
                _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                return NotFound(_MessageLocalizer["Company not found"]);
            }

            bool operationResult = true;
            operationResult &= await DisableEmployeesForTheCompany(companyData);
            if (operationResult) {
                companyData.Active = true;
                operationResult &= await _UnitOfWork.Companies.UpdateAsync(companyData);
            }
            operationResult &= await _UnitOfWork.Save();
            if (!operationResult) {
                ModelState.AddModelError("", "Unabled to lock company.");
                await SendNotificationToAppAdmins($"Unable to lock company #{id} ({companyData.CompanyName})",
                    subject: $"Company {companyData.CompanyName} deactivating emergency");
                return RedirectToAction(nameof(Details), new { id });
            }
            else
                return RedirectToAction(nameof(Index), new { showDisabled = true });
        }

        private async Task<bool> DisableEmployeesForTheCompany(Company companyData) {
            if (companyData == null)
                return false;
            bool result = true;
            var employeesOfTheCompany = await _UnitOfWork.Employees.WhereAsync(emp => emp.CompanyId == companyData.Id);
            StringBuilder lockedEmployees = new StringBuilder();
            foreach (var employee in employeesOfTheCompany) {
                if (!await(_UserManager.IsMemberOfOneAsync(employee, UserRoles.CompanyAdministrator))) {
                    employee.LockoutEnabled = companyData.EnableLockoutForEmployees;
                    employee.LockoutEnd = DateTime.Now;
                    result &= await _UnitOfWork.Employees.UpdateAsync(employee);
                    if (result)
                        lockedEmployees.Append($"{{ id:{employee.Id}, \nuserName: {employee.UserName} }}");
                    else
                        break;
                }
            }
            return result;
        }

        public async Task<ActionResult> PermanentDeleteCompany(int id) {
            var currentUser = await _UserManager.GetUserAsync(User);
            UserRoles currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(currentUser.Id));
            bool roleAllowsEditCompanyData = ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                || (((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator) && currentEmployee?.CompanyId != id);
            if (!roleAllowsEditCompanyData) {
                _CompanyControllerLogger.LogWarning($"User {currentUser.UserName} was forbidden to browse companies");
                ModelState.AddModelError("", _MessageLocalizer["Your not allowed to show the details of this company"]);
                return Forbid(_MessageLocalizer["Your not allowed to show the details of this company"]);
            }
            var companyData = await _UnitOfWork.Companies.FindAsync(x=>x.Id == id);
            if (companyData == null) {
                _CompanyControllerLogger.LogWarning($"Company {id} was not found");
                ModelState.AddModelError("", _MessageLocalizer["Company not found"]);
                return NotFound(_MessageLocalizer["Company not found"]);
            }
            bool operationResult = true;
            var leaveTypesToRemove = await _UnitOfWork.LeaveTypes.WhereAsync(filter: lt => lt.CompanyId == id);
            foreach (var leaveType in leaveTypesToRemove) {
                (await _UnitOfWork.LeaveAllocations.WhereAsync(filter: x => x.AllocationLeaveTypeId == leaveType.Id))?.
                    ToList().ForEach( async (el) => {
                        operationResult &= operationResult && await _UnitOfWork.LeaveAllocations.DeleteAsync(el);
                    });
                (await _UnitOfWork.LeaveRequest.WhereAsync(filter: x => x.LeaveTypeId == leaveType.Id))?.
                    ToList().ForEach(async (el) => {
                        operationResult &= operationResult && await _UnitOfWork.LeaveRequest.DeleteAsync(el);
                    });
                operationResult &= operationResult && await _UnitOfWork.LeaveTypes.DeleteAsync(leaveType);
            }
            (await _UnitOfWork.Employees.WhereAsync(x => x.CompanyId == id)).ToList().ForEach(async (empl) => {
                operationResult &= operationResult && await _UnitOfWork.Employees.DeleteAsync(empl);
            });
            operationResult &= await _UnitOfWork.Companies.DeleteAsync(companyData);
            operationResult &= await _UnitOfWork.Save();
            if (!operationResult) {
                await SendNotificationToAppAdmins($"Failed to remove company data while permanently removing company {companyData.Id}",
                            $"Emergency for the company #{id} ({companyData.CompanyName})");
                ModelState.AddModelError("", _MessageLocalizer["Failed to remove company. Administrator was notified about this problem, he will remove the rests of data in more bref delays"]);
                return await Details(companyData.Id);
            }
            if ((currentUserRoles & UserRoles.AppAdministrator) != UserRoles.AppAdministrator)
                await _SignInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        public async Task SendNotificationToAppAdmins(string message, string subject) {
            var users = await _UserManager.GetUsersInRoleAsync(UserRoles.AppAdministrator.ToString());
            foreach (var user in users) {
                var employee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(user.Id));
                string email = !String.IsNullOrWhiteSpace(employee?.ContactMail) ? employee?.ContactMail : (user.EmailConfirmed ? user.Email : String.Empty);
                if (!String.IsNullOrWhiteSpace(email))
                    await _EmailSender.SendEmailAsync(email, subject, message);
            }

        }

        #region Disposing

        public new void Dispose() {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        private bool _Disposed = false;

        protected override void Dispose(bool disposing) {
            if (_Disposed)
                return;

            if (disposing) {
                _UnitOfWork.Dispose();
            }
            base.Dispose(disposing);

            _Disposed = true;
        }

        #endregion
    }
}

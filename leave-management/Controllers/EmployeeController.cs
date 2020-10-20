using AutoMapper;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.PasswordGenerator;
using LeaveManagement.Repository.Entity;
using LeaveManagement.ViewModels.Employee;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Resources;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace LeaveManagement.Controllers {
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class EmployeeController : Controller {

        private const string RegisterEmployeeView = "Create";
        private const string EditEmployeeView = "Edit";

        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly ILogger<EmployeePresentationDefaultViewModel> _Logger;
        private readonly IEmailSender _EmailSender;
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly IStringLocalizer _DataLocalizer;
        private readonly IMapper _Mapper;
        private readonly IPasswordGenerator _PasswordGenerator;

        public EmployeeController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EmployeePresentationDefaultViewModel> logger,
            IEmailSender emailSender,
            ILeaveManagementUnitOfWork unitOfWork,
            IStringLocalizerFactory localizerFactory,
            IMapper mapper,
            IPasswordGenerator passwordGenerator) {
            _SignInManager = signInManager;
            _UserManager = userManager;
            _RoleManager = roleManager;
            _Logger = logger;
            _EmailSender = emailSender;
            _DataLocalizer = localizerFactory.Create(typeof(Areas.Identity.Pages.Account.RegisterModel));
            _Mapper = mapper;
            _PasswordGenerator = passwordGenerator;
            _UnitOfWork = unitOfWork;
        }

        string ReturnUrl = "";
        #region Service methods
        private async Task<UserRoles> GetAllowedRolesAsync(UserRoles currentUserMembership, bool self) {
            return await Task.Run(() => {
                UserRoles result = UserRoles.None;
                if ((currentUserMembership & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                    result |= (UserRoles.CompanyAdministrator | UserRoles.AppAdministrator);
                if ((currentUserMembership & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator)
                    result |= (UserRoles.CompanyAdministrator | UserRoles.Employee | UserRoles.HRManager);
                if ((currentUserMembership & UserRoles.HRManager) == UserRoles.HRManager)
                    result |= UserRoles.Employee;
                if (self)
                    result |= currentUserMembership;
                return result;
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetListAllowedRoles(UserRoles currentUserRoles = UserRoles.None, bool self = false) {
            UserRoles allowedRoles = await GetAllowedRolesAsync(currentUserRoles, self);
            var userRolesValues = Enum.GetValues(typeof(UserRoles));
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (object userRoleObj in userRolesValues) {
                UserRoles userRole = (UserRoles)userRoleObj;
                selectListItems.Add(new SelectListItem() {
                    Text = _DataLocalizer[userRole.ToString()],
                    Value = userRole.ToString(),
                    Selected = (currentUserRoles & userRole) == userRole,
                    Disabled = (userRole & allowedRoles) == UserRoles.None
                });
            }
            return selectListItems;
        }
        #endregion


        #region Creating Employees
        [HttpGet]
        public async Task<IActionResult> CreateEmployee() {
            ReturnUrl = HttpContext.Request.Headers["Referer"];
            var currentCredentials = await GetCurrentUserData();
            var editionPermissions = await GetEditionPermission(null);
            if (!editionPermissions.AllowEdition) {
                ModelState.AddModelError("", _DataLocalizer["Action permitted only to administrators"]);
                return Forbid();
            }
            EmployeeCreationVM employeeCreationVM = new EmployeeCreationVM() {
                EmploymentDate = DateTime.Now,
                AcceptContract = false,
                ReturnUrl = Request.Headers["Referer"].ToString(),
                RolesList = await GetListAllowedRoles(),
                Roles = new List<string>()
            };
            //
            if (currentCredentials.Item2?.CompanyId != null) {
                employeeCreationVM.CompanyId = currentCredentials.Item2.CompanyId;
                if (currentCredentials.Item2?.Company != null)
                    employeeCreationVM.Company = currentCredentials.Item2.Company;
                else
                    employeeCreationVM.Company = await _UnitOfWork.Companies.FindAsync(x=>x.Id == (int)employeeCreationVM.CompanyId);
            }
            //Case when new employee created inside the company
            employeeCreationVM = await SetEmployeeCompanyAndManagerState(employeeCreationVM, editionPermissions);
            employeeCreationVM.RolesListEnabled = editionPermissions.AllowEditAccess;
            employeeCreationVM.RolesList = await GetListAllowedRoles(currentCredentials.Item3, self: editionPermissions.IsSelfEdition);
            ViewBag.Action = nameof(CreateEmployee);
            return View(RegisterEmployeeView, employeeCreationVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeCreationVM employeeCreationVM) {

            string referer = employeeCreationVM.ReturnUrl;
            var currentPermissions = await GetEditionPermission(null);
            var currentUserData = await GetCurrentUserData();
            //Validating access
            if (!ValidateCreationAccess(currentUserData.Item3))
                return Forbid();
            //Validating duplicates
            await ValidateDuplicateMail(employeeCreationVM.Email);
            await ValidateDuplicateUserName(employeeCreationVM.UserName);
            //Validating contract
            ValidateContractAcceptence(employeeCreationVM.AcceptContract);
            //Validating is company and manager correctly assigned
            await ValidateEmployeeCreationVMState(employeeCreationVM);
            UserRoles employeeRoles = await ValidateAssignedRoles(UserManagerExtensions.ToUserRoles(employeeCreationVM.Roles), null);
            if (ModelState.ErrorCount > 0) {
                employeeCreationVM.RolesList = await GetListAllowedRoles(employeeRoles);
            }
            if (ModelState.ErrorCount == 0) {
                Employee employee = new Employee();
                employeeCreationVM.Id = employee.Id;
                employee = (Employee)_Mapper.Map(employeeCreationVM, typeof(EmployeeCreationVM), typeof(Employee));
                string password = _PasswordGenerator.GeneratePassword();
                employee.LockoutEnabled = false;
                employee.EmailConfirmed = true;
                bool result = await _UnitOfWork.Employees.RegisterEmployeeAsync(_UserManager, employee, password);
                if (!result)
                    ModelState.AddModelError("", _DataLocalizer["Unabled to save employee into repository"]);
                else
                    await SendMailWithCredentials(employee, password);
            }
            //Validating user roles
            if (ModelState.ErrorCount == 0) {
                var employeeIdentityUser = await _UserManager.FindByEmailAsync(employeeCreationVM.Email);
                var newRoles = UserManagerExtensions.FromUserRoles(employeeRoles).ToList();
                foreach (var role in newRoles) {
                    var identityResult = await _UserManager.AddToRoleAsync(employeeIdentityUser, role);
                    if (!identityResult.Succeeded)
                        ModelState.AddModelError("", _DataLocalizer["Unable to add this user to role {0}", role]);
                };
            }

            if (ModelState.ErrorCount > 0) {
                employeeCreationVM = await SetEmployeeCompanyAndManagerState(employeeCreationVM, currentPermissions);
                employeeCreationVM.RolesListEnabled = currentPermissions.AllowEditAccess;
                employeeCreationVM.RolesList = await GetListAllowedRoles(currentUserData.Item3, self: currentPermissions.IsSelfEdition);
                ViewData["Referer"] = referer;
                ViewBag.Action = nameof(CreateEmployee);
                return View(RegisterEmployeeView, employeeCreationVM);
            }
            else {
                if (!String.IsNullOrEmpty(referer))
                    return Redirect(referer);
                else
                    return RedirectToAction("Index", "Home");
            }

        }

        private bool ValidateCreationAccess(UserRoles currentUserRoles) {
            bool allowed = (currentUserRoles & (UserRoles.AppAdministrator | UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;
            if (!allowed) {
                ModelState.AddModelError("", _DataLocalizer["Action permitted only to administrators"]);
            }
            return allowed;
        }

        private async Task<bool> ValidateDuplicateMail(string email) {
            bool valid = (await _UserManager.FindByEmailAsync(email)) == null;
            if (!valid) {
                ModelState.AddModelError("", _DataLocalizer["User with same email already exists"]);
            }
            return valid;
        }

        private async Task<bool> ValidateDuplicateUserName(string userName) {
            bool valid = (await _UserManager.FindByNameAsync(userName)) == null;
            if (!valid) {
                ModelState.AddModelError("", _DataLocalizer["User with same email already exists"]);
            }
            return valid;
        }

        private async Task<UserRoles> ValidateAssignedRoles(UserRoles assignedRoles, Employee consernedEmployee) {
            var currentUserData = await GetCurrentUserData();
            UserRoles allowedRoles = await GetAllowedRolesAsync(currentUserData.Item3, consernedEmployee?.Id?.Equals(currentUserData.Item1.Id) ?? false);
            //Validating role assignement
            if ((assignedRoles ^ allowedRoles) > 0) {
                var currentUser = await _UserManager.GetUserAsync(User);
                _Logger.LogWarning($"Attempting assign of disabled roles for user {currentUser.UserName}");
            }

            assignedRoles &= allowedRoles; // Removing not allowed roles;
            return assignedRoles;
        }

        private void ValidateContractAcceptence(bool acceptContract) {
            if (!acceptContract) {
                ModelState.AddModelError("", _DataLocalizer["You must accept the contract"]);
            }
        }

        //Todo: Review this section 
        private async Task<EmployeeCreationVM> SetEmployeeCompanyAndManagerState(EmployeeCreationVM employeeCreationVM, EditionPermissions editionPermissions) {
            //Case when employee created by app admin
            var currentLoginData = await GetCurrentUserData();
            var currentUserRoles = currentLoginData.Item3;
            var currentEmployee = currentLoginData.Item2;
            bool currentUserIsAppAdmin = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            bool currentUserIsCompanyPriveleged = (currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;

            employeeCreationVM.CompanyEnabled = editionPermissions.AllowChangeCompany;
            if (editionPermissions.AllowChangeCompany) {
                var companiesList = (await _UnitOfWork.Companies.WhereAsync(x => x.Active))
                    .Select(x => new SelectListItem(x.CompanyName, x.Id.ToString(), x.Id.Equals(employeeCreationVM?.CompanyId))).ToList();
                companiesList.Insert(0, new SelectListItem(_DataLocalizer["Please select the company"], "0", true, true));
                companiesList.Insert(1, new SelectListItem(_DataLocalizer["None"], String.Empty, true, !currentUserIsAppAdmin));
                employeeCreationVM.Companies = companiesList;
            }
            else {
                if (currentEmployee?.CompanyId != null) {
                    if(employeeCreationVM.CompanyId == null)
                        employeeCreationVM.CompanyId = currentEmployee.CompanyId;
                    if (employeeCreationVM.CompanyId != null) {
                        if (currentEmployee?.Company != null)
                            employeeCreationVM.Company = currentEmployee.Company;
                        else
                            employeeCreationVM.Company = await _UnitOfWork.Companies.FindAsync(x=>x.Id == (int)employeeCreationVM.CompanyId);
                        employeeCreationVM.Companies = new SelectListItem[] {new SelectListItem(employeeCreationVM.Company.CompanyName,
                        employeeCreationVM.Company.Id.ToString(), true, true) };
                    }
                }
            }
            employeeCreationVM.ManagerEnabled = editionPermissions.AllowChangeManager;
            if (editionPermissions.AllowChangeManager) {
                var managers = await GetEmployeesByCompany(employeeCreationVM.CompanyId, employeeCreationVM.ManagerId);
                managers.Insert(0, new SelectListItem(_DataLocalizer["None"], null));
                employeeCreationVM.Managers = managers;
            }

            return employeeCreationVM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeCreationVM"></param>
        /// <param name="currentUserRoles"></param>
        /// <remarks>
        ///  - Validate company - company must be enabled
        ///  V - Validate compare with original employee if exists - in this case operation will fail
        ///  V - Validate if editor who is not admin was unabled change the company, if company is not null
        ///  - Validate manager from same company
        /// </remarks>
        /// <returns></returns>
        private async Task<bool> ValidateEmployeeCreationVMState(EmployeeCreationVM employeeCreationVM) {
            if (ModelState.ErrorCount > 0)
                return false;
            var currentUserData = await GetCurrentUserData();
            bool isCurrentUserIsAppAdmin = (currentUserData.Item3 & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            bool isCurrentUserIsCompanyAdmin = (currentUserData.Item3 & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;
            Employee potentialConsernedEmployee;
            if (!String.IsNullOrWhiteSpace(employeeCreationVM?.Id))
                potentialConsernedEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(employeeCreationVM.Id));
            else
                potentialConsernedEmployee = null;

            int? companyId = (employeeCreationVM.CompanyId != null && employeeCreationVM.CompanyId > 0) ? employeeCreationVM.CompanyId : null;
            if (companyId != null) {
                if (!isCurrentUserIsAppAdmin &&  !(currentUserData.Item2?.CompanyId?.Equals(companyId) ?? false))
                    ModelState.AddModelError("", _DataLocalizer["Only app admin can assign employee not from his company"]);
                var company = await _UnitOfWork.Companies.FindAsync(x=>x.Id==(int)companyId);
                if( !(company?.Active??false))
                    ModelState.AddModelError("", _DataLocalizer["Company inactive or not found"]);
            }
            if(!(isCurrentUserIsCompanyAdmin || isCurrentUserIsAppAdmin)) {
                ModelState.AddModelError("", _DataLocalizer["Action forbidden to not priveleged Employee"]);
            }
            
            if(companyId != null) {
                if (!string.IsNullOrWhiteSpace(employeeCreationVM.ManagerId)) {
                    string managerId = employeeCreationVM.ManagerId;
                    Employee manager = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(managerId));
                    if(!(isCurrentUserIsCompanyAdmin || isCurrentUserIsAppAdmin))
                        ModelState.AddModelError("", _DataLocalizer["You not allowed to assign manager"]);
                    if (manager == null)
                        ModelState.AddModelError("", _DataLocalizer["Manager not found"]);
                    else if (!manager.Id.Equals(managerId))
                        ModelState.AddModelError("", _DataLocalizer["Manager nust be from same company as employee"]);
                }
                
            }
            return ModelState.ErrorCount == 0;
        }

        [HttpPost]
        public async Task<IList<SelectListItem>> GetEmployeesByCompany(int? companyId, string managerId) {
            if (string.IsNullOrWhiteSpace(managerId))
                managerId = string.Empty;
            ICollection<Employee> employees;
            if (companyId != null)
                employees = await _UnitOfWork.Employees.WhereAsync(filter: empl => empl.CompanyId == companyId,
                    includes : new System.Linq.Expressions.Expression<Func<Employee, object>>[] {x=>x.Company, x=>x.Manager });
            else
                employees = await _UnitOfWork.Employees.WhereAsync(filter: empl => empl.CompanyId == null,
                    includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
            IList<SelectListItem> result = new List<SelectListItem>();
            if (employees.Count > 0)
                result = employees.Select(x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, managerId.Equals(x.Id))).ToList();
            return result;
        }


        #endregion

        #region Updating employees

        #region Getting data about actually connected user
        Tuple<IdentityUser, Employee, UserRoles> _IdentityData = null;
        object identityDataLock = new object();


        private async Task<Tuple<IdentityUser, Employee, UserRoles>> GetCurrentUserData() {
            if (_IdentityData != null)
                return _IdentityData;
            else {
                var currentUser = await _UserManager.GetUserAsync(User);
                var currentEmployee = await _UnitOfWork.Employees.FindAsync( x=>x.Id.Equals(currentUser.Id),
                    includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x=>x.Company, x=>x.Manager });
                UserRoles userRoles = await _UserManager.GetUserRolesAsync(currentUser);
                lock (identityDataLock) {
                    _IdentityData = Tuple.Create(currentUser, currentEmployee, userRoles);
                }
                return _IdentityData;
            }

        }
        #endregion

        protected class EditionPermissions {

            public static EditionPermissions AllGranted => new EditionPermissions() {
                AllowChangeCompany = true,
                AllowChangeManager = true,
                AllowEditAccess = true,
                AllowEditAccount = true,
                AllowEditContacts = true,
                AllowEdition = true,
                AllowEditProfile = true,
                AllowRemoveAccount = true
            };

            public static EditionPermissions AllForbidden => new EditionPermissions();

            public bool AllowEdition;
            public bool AllowEditAccount;
            public bool AllowEditProfile;
            public bool AllowEditContacts;
            public bool AllowEditAccess;
            public bool AllowChangeManager;
            public bool AllowChangeCompany;
            public bool IsSelfEdition;
            public bool AllowRemoveAccount;

        }

        private async Task<EditionPermissions> GetEditionPermission(
            Employee concernedEmployee) {
            ///User allowed edit account in the case when
            /// - Is not employee but company admin
            /// - Is redactor is privileged user from same company
            /// - If recactor ant concerned user is same persone
            var currentUserData = await GetCurrentUserData();
            var currentUser = currentUserData.Item1;
            var currentUserRoles = currentUserData.Item3;
            var currentEmployee = currentUserData.Item2;
            bool isCompanyPriveleged = (currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;
            bool isAppAdministrator = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            if (concernedEmployee == null) {
                if (isCompanyPriveleged || isAppAdministrator) {
                    EditionPermissions permissions = EditionPermissions.AllGranted;
                    permissions.AllowChangeCompany = isAppAdministrator;
                    permissions.AllowEditAccess = isAppAdministrator || ((currentUserRoles & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator);
                    permissions.IsSelfEdition = false;
                    return permissions;
                }
                else
                    return EditionPermissions.AllForbidden;
            }
            bool allowEdition = (currentUser != null && (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
            ||
            (currentEmployee != null && currentEmployee.CompanyId == concernedEmployee?.CompanyId && ((currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0))
            || currentEmployee != null && currentEmployee.Id == concernedEmployee?.Id;
            if (currentUser == null || !allowEdition)
                return new EditionPermissions() { AllowEdition = false };

            bool isItSelfEdition = concernedEmployee?.Id.Equals(currentUser.Id) ?? false;
            bool isItSameCompanyEdition = concernedEmployee.CompanyId == currentEmployee?.CompanyId;
            bool allowEditProfile = (isCompanyPriveleged & isItSameCompanyEdition) || isAppAdministrator;
            bool allowEditAccess = await GetAllowedRolesAsync(currentUserRoles, isItSelfEdition) != UserRoles.None;
            bool allowEditContact = isAppAdministrator || (isItSameCompanyEdition && isCompanyPriveleged) || isItSelfEdition;
            bool allowChangeManager = (isCompanyPriveleged && isItSameCompanyEdition) || isAppAdministrator;
            bool allowChangeCompany = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            bool allowRemoveProfile = (currentUserRoles & (UserRoles.AppAdministrator| UserRoles.CompanyAdministrator)) > UserRoles.None;
            return new EditionPermissions() {
                AllowEdition = allowEdition,
                IsSelfEdition = isItSelfEdition,
                AllowEditAccess = allowEditAccess,
                AllowEditProfile = allowEditProfile,
                AllowEditContacts = allowEditContact,
                AllowChangeManager = allowChangeManager,
                AllowChangeCompany = allowChangeCompany,
                AllowRemoveAccount = allowRemoveProfile
            };
        }

        #region Validation
        private bool ValidateProfileEdition(Employee consernedEmployee, EmployeeCreationVM employeeVM, bool allow) {
            bool valid = true;
            valid &= allow || (consernedEmployee.Title?.Equals(employeeVM.Title) ?? false);
            valid &= allow || (consernedEmployee.FirstName?.Equals(employeeVM.FirstName) ?? false);
            valid &= allow || (consernedEmployee.LastName?.Equals(employeeVM.LastName) ?? false);
            valid &= allow || (consernedEmployee.TaxRate?.Equals(employeeVM.TaxRate) ?? false);
            valid &= allow || (consernedEmployee.DateOfBirth.Equals(employeeVM.DateOfBirth));
            return valid;
        }

        private async Task<bool> ValidateCompanyData(Employee consernedEmployee, EmployeeCreationVM employeeVM, bool allow) {
            bool result = allow || consernedEmployee.CompanyId == employeeVM.CompanyId;
            UserRoles consernedEmployeeRoles;
            if (employeeVM.Roles == null)
                consernedEmployeeRoles = await _UserManager.GetUserRolesAsync(consernedEmployee);
            else
                consernedEmployeeRoles = UserManagerExtensions.ToUserRoles(employeeVM.Roles);
            bool isAppAdministrator = (consernedEmployeeRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;
            if (!isAppAdministrator && employeeVM.CompanyId == default) {
                ModelState.AddModelError("", _DataLocalizer["Company is required for all employees who is not app administrator"]);
                return false;
            }
            return result || isAppAdministrator;
        }

        private bool ValidateManagerData(Employee employee, EmployeeCreationVM employeeVM, bool allow) {
            return allow || employee.ManagerId == employeeVM.ManagerId;
        }

        private bool ValidateContactData(Employee employee, EmployeeCreationVM employeeVM, bool allow) {
            bool valid = true;
            valid &= allow || employee.PhoneNumber.Equals(employeeVM.PhoneNumber);
            valid &= allow || employee.ContactMail.Equals(employeeVM.ContactMail);
            valid &= employee.Email.Equals(employeeVM.Email);
            return valid;
        }

        private async Task<bool> ValidateRoles(Employee employee, EmployeeCreationVM employeeVM, bool allow) {
            bool validRoles;
            UserRoles employeeRoles = await _UserManager.GetUserRolesAsync(employee);
            if (employeeVM.Roles == null || !employeeVM.RolesListEnabled)
                return true;
            UserRoles employeeVmRoles = UserManagerExtensions.ToUserRoles(employeeVM.Roles);
            validRoles = allow || (employeeRoles == employeeVmRoles);
            if (validRoles)
                validRoles |= allow && ((await ValidateAssignedRoles(employeeVmRoles, employee)) == employeeVmRoles);
            return validRoles;
        }

        private async Task<bool> ValidateInput(Employee consernedEmployee, EmployeeCreationVM employeeVM, EditionPermissions permission) {
            if (consernedEmployee == null || !permission.AllowEdition)
                return false;
            bool inputValid = permission.AllowEdition;
            bool rolesListValid = await ValidateRoles(consernedEmployee, employeeVM, permission.AllowEditAccess);
            if (!rolesListValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change the roles"]);
            inputValid &= rolesListValid;
            bool profileValid = ValidateProfileEdition(consernedEmployee, employeeVM, permission.AllowEditProfile);
            if (!profileValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit profile data"]);
            inputValid &= profileValid;
            bool contactsValid = ValidateContactData(consernedEmployee, employeeVM, permission.AllowEditContacts);
            if (!contactsValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit contacts data"]);
            inputValid &= contactsValid;
            bool managerValid = ValidateManagerData(consernedEmployee, employeeVM, permission.AllowChangeManager);
            if (!managerValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change your manager"]);
            inputValid &= managerValid;
            bool companyValid = await ValidateCompanyData(consernedEmployee, employeeVM, permission.AllowChangeCompany);
            if (!companyValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change your manager"]);
            inputValid &= companyValid;

            return inputValid;
        }
        #endregion

        #region Preparing edition view model
        private async Task<EmployeeCreationVM> PrepareEmployeeEditionViewModel(EmployeeCreationVM employeeCreationVM,
           Employee concernedEmployee, UserRoles concernedEmployeesRoles,
           EditionPermissions editionPermissions) {
            var currentUserData = await GetCurrentUserData();
            if (editionPermissions == null)
                editionPermissions = await GetEditionPermission(concernedEmployee);
            employeeCreationVM = await SetEmployeeCompanyAndManagerState(employeeCreationVM, editionPermissions);
            employeeCreationVM.RolesListEnabled = !concernedEmployee.Id.Equals(currentUserData.Item1.Id);
            employeeCreationVM.RolesList = await GetListAllowedRoles(currentUserData.Item3);
            employeeCreationVM.Roles = UserManagerExtensions.FromUserRoles(concernedEmployeesRoles).ToList();
            employeeCreationVM.ManagerEnabled = editionPermissions.AllowChangeManager;
            employeeCreationVM.CompanyEnabled = editionPermissions.AllowChangeCompany;
            return employeeCreationVM;
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(string employeeId) {
            var currentUserData = await GetCurrentUserData();
            if (currentUserData.Item1 == null)
                return Forbid();

            IdentityUser consernedUser;
            Employee consernedEmployee;
            if (!String.IsNullOrWhiteSpace(employeeId)) {
                consernedUser = await _UserManager.FindByIdAsync(employeeId);
                consernedEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(employeeId),
                    includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
            }
            else {
                consernedUser = currentUserData.Item1;
                consernedEmployee = currentUserData.Item2;
            }
            
            if (consernedUser == null)
                return NotFound();
            if (consernedUser != null && consernedEmployee == null && (currentUserData.Item3 & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                return await UpdateIdentityUser(consernedUser);

            var editionPermission = await GetEditionPermission(consernedEmployee);
            if (!editionPermission.AllowEdition)
                return Forbid();
            var employeeRoles = await _UserManager.GetUserRolesAsync(consernedUser);
            var editionViewModel = _Mapper.Map<EmployeeCreationVM>(consernedEmployee);
            editionViewModel = await PrepareEmployeeEditionViewModel(editionViewModel, consernedEmployee, employeeRoles,
                editionPermission);
            @ViewBag.Action = nameof(UpdateEmployee);
            return View(EditEmployeeView, editionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(EmployeeCreationVM employeeCreationVM) {
            var consernedEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(employeeCreationVM.Id),
                includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager }
            );
            if (consernedEmployee == null)
                return NotFound();
            employeeCreationVM.UserName = consernedEmployee.UserName;
            employeeCreationVM.Email = consernedEmployee.Email;
            var currentUserData = await GetCurrentUserData();
            bool reviewUserRoles = currentUserData.Item3 > UserRoles.Employee;
            bool canEditUser = reviewUserRoles || employeeCreationVM.Id.Equals(currentUserData.Item1.Id);
            if (!canEditUser) {
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit this profile"]);
            }
            var permissions = await GetEditionPermission(consernedEmployee);
            bool validation = await ValidateInput(consernedEmployee, employeeCreationVM, permissions);
            if (ModelState.ErrorCount == 0 && validation) {
                //Reappliying company which was not changed
                if (!permissions.AllowChangeCompany)
                    employeeCreationVM.CompanyId = consernedEmployee.CompanyId;
                if (!permissions.AllowChangeManager)
                    employeeCreationVM.ManagerId = consernedEmployee.ManagerId;
                consernedEmployee = _Mapper.Map(employeeCreationVM, consernedEmployee);
                bool result = await _UnitOfWork.Employees.UpdateAsync(consernedEmployee);
                result &= await _UnitOfWork.Save();
                if (!result) {
                    ModelState.AddModelError("", _DataLocalizer["Unable to save user to repository"]);
                }
                else {
                    result &= await UpdateUserRoles(consernedEmployee, employeeCreationVM.Roles);
                }
                if (currentUserData.Item3 <= UserRoles.Employee)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index");
            }
            else {
                @ViewBag.Action = nameof(UpdateEmployee);
                employeeCreationVM = await PrepareEmployeeEditionViewModel(employeeCreationVM: employeeCreationVM,
                    concernedEmployee: consernedEmployee,
                    concernedEmployeesRoles: await _UserManager.GetUserRolesAsync(consernedEmployee),
                    editionPermissions: permissions);
                return View(EditEmployeeView, employeeCreationVM);
            }
        }



        private async Task<bool> UpdateUserRoles(IdentityUser employee, ICollection<string> userRoles) {
            if (userRoles == null)
                return true;
            bool updateResult = true;
            UserRoles newRoles = UserManagerExtensions.ToUserRoles(userRoles);
            UserRoles oldRoles = await _UserManager.GetUserRolesAsync(employee);
            var addedRoles = UserManagerExtensions.FromUserRoles((oldRoles ^ newRoles) & newRoles);
            var removedRoles = UserManagerExtensions.FromUserRoles((oldRoles ^ newRoles) & oldRoles);
            updateResult &= (await _UserManager.AddToRolesAsync(employee, addedRoles)).Succeeded;
            updateResult &= (await _UserManager.RemoveFromRolesAsync(employee, removedRoles)).Succeeded;
            return updateResult;
        }

        public async Task<IActionResult> UpdateIdentityUser(IdentityUser user) {
            return await Task.Run(() =>Redirect("~/Identity/Account/Manage/Index"));
        }
        #endregion

        #region Index

        public async Task<ActionResult> IndexForYourCompany() {
            var currentUser = await _UserManager.GetUserAsync(User);
            if (currentUser == null) {
                ModelState.AddModelError("", _DataLocalizer["You not authorized to browse employees"]);
                return Forbid();
            }
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(currentUser.Id),
                includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
            if (currentEmployee == null) {
                ModelState.AddModelError("", _DataLocalizer["You not authorized to browse employees"]);
                return await Index(null);
            }
            else
                return await Index(currentEmployee.CompanyId);
        }

        /// - List employees from same company
        /// - List employees without company
        /// - List of company admins (by company id)
        [HttpGet]
        public async Task<ActionResult> Index(int? companyId) {
            var currentUser = await _UserManager.GetUserAsync(User);
            if (currentUser == null) {
                ModelState.AddModelError("", _DataLocalizer["You not authorized to allocate leave to employees"]);
                return Forbid();
            }
            IList<Employee> selectedEmployees = null;
            var currentEmployeeRoles = await _UserManager.GetUserRolesAsync(User);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(currentUser.Id),
                includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
            ///Company admin can browse all
            if ((currentEmployeeRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator) {
                if (companyId != null) {
                    var employeesForThisCompany = await _UnitOfWork.Employees.WhereAsync(
                        filter: x => x.CompanyId == (int)companyId,
                        order: o => o.OrderBy(e => e.LastName).ThenBy(e => e.FirstName),
                        includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
                    selectedEmployees = employeesForThisCompany.ToList();
                }
                else {
                    selectedEmployees = (await _UnitOfWork.Employees.WhereAsync(x => true,
                        order: o => o.OrderBy(e => e.LastName).ThenBy(e => e.FirstName),
                        includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager }
                        )).ToList();
                }
            }
            ///If user is not member of app administrator, but priveleged inside the company, he can still see the users from his company
            else if (currentEmployee != null && ((currentEmployeeRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0)) {
                selectedEmployees = (await _UnitOfWork.Employees.WhereAsync(filter: x => x.CompanyId == currentEmployee.CompanyId,
                    order: o => o.OrderBy(e => e.LastName).ThenBy(e => e.FirstName),
                    includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager })).ToList();
            }
            else
                return Forbid();
            var employeesList = new List<EmployeePresentationDefaultViewModel>();
            foreach (Employee employee in selectedEmployees) {
                var employeePresentation = _Mapper.Map<EmployeePresentationDefaultViewModel>(employee);
                employeePresentation.CanAllocateLeave = employee.CompanyId == currentEmployee?.CompanyId;
                employeesList.Add(employeePresentation);
            }
            return View(employeesList);
        }
        #endregion

        public async Task<ActionResult> Details(string userId) {
            var currentUserData = await GetCurrentUserData();
            if (currentUserData?.Item1 == null)
                return NotFound();
            if (String.IsNullOrWhiteSpace(userId))
                userId = currentUserData.Item1.Id;
            if (currentUserData.Item1.Id.Equals(userId)
                || await _UserManager.IsCompanyPrivelegedUser(currentUserData.Item3)
                || ((currentUserData.Item3 & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)) {
                Employee consernedEmployee;
                if (currentUserData.Item1.Id.Equals(userId))
                    consernedEmployee = currentUserData.Item2 != null ? currentUserData.Item2 : null;
                else
                    consernedEmployee = await _UnitOfWork.Employees.FindAsync(predicate: x => x.Id.Equals(userId),
                        includes: new System.Linq.Expressions.Expression<Func<Employee, object>>[] { x => x.Company, x => x.Manager });
                if (consernedEmployee != null) {
                    EmployeeCreationVM employeeCreationVM = _Mapper.Map<EmployeeCreationVM>(consernedEmployee);
                    return View(employeeCreationVM);
                }
                else {
                    return RedirectToPage("~/Identity/Account/Manage/Index", new { userId = currentUserData.Item1.Id });
                }
            }
            else
                return Forbid();
        }

        public async Task<ActionResult> RemoveEmployee(string employeeId) {
            var consernedEmployee = await _UnitOfWork.Employees.FindAsync(
                predicate: x=>x.Id.Equals(employeeId));
            if (consernedEmployee == null)
                return NotFound();
            var currentUserRights = await GetEditionPermission(consernedEmployee);
            bool result = true;
            int? companyId = consernedEmployee.CompanyId;
            if (currentUserRights.AllowRemoveAccount) {
                result &= await _UnitOfWork.Employees.DeleteAsync(consernedEmployee);
                result &= await _UnitOfWork.Save();
            }
            if (!result)
                return RedirectToAction(nameof(Details), new { userId = employeeId });
            else
                return RedirectToAction(nameof(Index), new {companyId = companyId } );
        }

        #region Changing password
        [HttpGet]
        public async Task<ActionResult> ChangePassword() {
            return await Task.FromResult(View(new EmployeePassword()));
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(EmployeePassword employeePassword) {
            if (!ModelState.IsValid)
                return View(new EmployeePassword());
            if (!employeePassword.Password.Equals(employeePassword.ConfirmPassword)) {
                ModelState.AddModelError("", _DataLocalizer["Password and confirmation not matches"]);
                return View(employeePassword);
            }
            var currentUser = await _UserManager.GetUserAsync(User);
            var result = await _UserManager.ChangePasswordAsync(currentUser, employeePassword.OldPassword, employeePassword.Password);
            if (!result.Succeeded) {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return View(new EmployeePassword());
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Simple methods
        /// <summary>
        /// This is simple method - I not wanted to integrate letters models, too long
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        private async Task SendMailWithCredentials(Employee newEmployee, string password) {
            string model = @"Hello, {0}!<br/> 
                From now you can post your leave by our LeaveManagement System.<br/>
                    Your user name is: {1}<br/>
                    password: {2}<br/>
                Have a nice day and see you later!";
            string letterText = _DataLocalizer[model, newEmployee.FormatEmployeeSNT(), newEmployee.Email, password];
            string subject = _DataLocalizer["Your account at LeaveManagement System was created"];
            string contactMail = String.IsNullOrWhiteSpace(newEmployee.ContactMail) ? newEmployee.Email : newEmployee.ContactMail;
            await _EmailSender.SendEmailAsync(contactMail, subject, letterText);
        }
        #endregion


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

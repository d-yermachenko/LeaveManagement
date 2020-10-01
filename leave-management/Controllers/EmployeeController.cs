using AutoMapper;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.PasswordGenerator;
using LeaveManagement.ViewModels.Employee;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly IStringLocalizer _DataLocalizer;
        private readonly IMapper _Mapper;
        private readonly IPasswordGenerator _PasswordGenerator;
        private readonly ICompanyRepository _CompanyRepository;

        public EmployeeController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EmployeePresentationDefaultViewModel> logger,
            IEmailSender emailSender,
            IEmployeeRepositoryAsync employeeRepository,
            IStringLocalizerFactory localizerFactory,
            IMapper mapper,
            IPasswordGenerator passwordGenerator,
            ICompanyRepository companyRepository) {
            _SignInManager = signInManager;
            _UserManager = userManager;
            _RoleManager = roleManager;
            _Logger = logger;
            _EmailSender = emailSender;
            _EmployeeRepository = employeeRepository;
            _DataLocalizer = localizerFactory.Create(typeof(Areas.Identity.Pages.Account.RegisterModel));
            _Mapper = mapper;
            _PasswordGenerator = passwordGenerator;
            _CompanyRepository = companyRepository;
        }

        string ReturnUrl = "";
        #region Service methods
        private async Task<UserRoles> GetAllowedRolesAsync() {
            UserRoles userMemberShip = await _UserManager.GetUserRolesAsync(User);
            UserRoles result = UserRoles.None;
            if ((userMemberShip & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                result |= UserRoles.CompanyAdministrator;
            if ((userMemberShip & UserRoles.CompanyAdministrator) == UserRoles.CompanyAdministrator)
                result |= (UserRoles.CompanyAdministrator | UserRoles.Employee | UserRoles.HRManager);
            if ((userMemberShip & UserRoles.HRManager) == UserRoles.HRManager)
                result |= UserRoles.Employee;

            return result;
        }

        private async Task<IEnumerable<SelectListItem>> GetListAllowedRoles(UserRoles assignedRoles = UserRoles.None) {
            UserRoles allowedRoles = await GetAllowedRolesAsync();
            var userRolesValues = Enum.GetValues(typeof(UserRoles));
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (object userRoleObj in userRolesValues) {
                UserRoles userRole = (UserRoles)userRoleObj;
                selectListItems.Add(new SelectListItem() {
                    Text = _DataLocalizer[userRole.ToString()],
                    Value = userRole.ToString(),
                    Selected = (assignedRoles & userRole) == userRole,
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
            var currentUser = await _UserManager.GetUserAsync(User);
            var currentEmployee = await _EmployeeRepository.GetEmployeeAsync(User);
            var currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var allowed = (currentUserRoles & (UserRoles.AppAdministrator | UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;
            if (!allowed) {
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
            //Case when new employee created inside the company
            employeeCreationVM = await SetEmployeeCreationVMState(employeeCreationVM, currentUserRoles, currentEmployee);
            ViewBag.Action = nameof(CreateEmployee);
            return View(RegisterEmployeeView, employeeCreationVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeCreationVM employeeCreationVM) {
            string referer = employeeCreationVM.ReturnUrl;
            var currentUser = await _UserManager.GetUserAsync(User);
            var currentEmployee = await _EmployeeRepository.GetEmployeeAsync(User);
            var currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            //Validating access
            if (!ValidateCreationAccess(currentUserRoles))
                return Forbid();
            //Validating duplicates
            await ValidateDuplicateMail(employeeCreationVM.Email);
            await ValidateDuplicateUserName(employeeCreationVM.UserName);
            UserRoles employeeRoles = LeaveManagementExtensions.ToUserRoles(employeeCreationVM.Roles);
            employeeRoles = await ValidateAssignedRoles(employeeRoles);
            //Validating contract
            ValidateContractAcceptence(employeeCreationVM.AcceptContract);
            //Validating is company and manager correctly assigned
            await ValidateEmployeeCreationVMState(employeeCreationVM, currentUserRoles);
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
                bool result = await _EmployeeRepository.RegisterEmployeeAsync(employee, password);
                if (!result)
                    ModelState.AddModelError("", _DataLocalizer["Unabled to save employee into repository"]);
                else
                    await SendMailWithCredentials(employee, password);
            }
            //Validating user roles
            if (ModelState.ErrorCount == 0) {
                var employeeIdentityUser = await _UserManager.FindByEmailAsync(employeeCreationVM.Email);
                var newRoles = LeaveManagementExtensions.FromUserRoles(employeeRoles).ToList();
                foreach (var role in newRoles) {
                    var identityResult = await _UserManager.AddToRoleAsync(employeeIdentityUser, role);
                    if (!identityResult.Succeeded)
                        ModelState.AddModelError("", _DataLocalizer["Unable to add this user to role {0}", role]);
                };
            }

            if (ModelState.ErrorCount > 0) {
                employeeCreationVM.RolesList = await GetListAllowedRoles();
                employeeCreationVM = await SetEmployeeCreationVMState(employeeCreationVM, currentUserRoles, currentEmployee);
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

        private async Task<UserRoles> ValidateAssignedRoles(UserRoles assignedRoles) {
            UserRoles allowedRoles = await GetAllowedRolesAsync();
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

        private async Task<EmployeeCreationVM> SetEmployeeCreationVMState(EmployeeCreationVM employeeCreationVM, UserRoles currentUserRoles,
            Employee currentEmployee) {
            //Case when employee created by app admin
            //Company selection allowed, but manager selection is awalaibel only if company is assigned
            if ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator) {
                employeeCreationVM.CompanyEnabled = true;
                var companiesList = (await _CompanyRepository.WhereAsync(x => x.Active))
                    .Select(x => new SelectListItem(x.CompanyName, x.Id.ToString())).ToList();
                companiesList.Insert(0, new SelectListItem(_DataLocalizer["Please select the company"], "0", true, true));
                employeeCreationVM.Companies = companiesList;
                employeeCreationVM.ManagerEnabled = true;
                employeeCreationVM.Managers = await GetEmployeesByCompany(employeeCreationVM.CompanyId, employeeCreationVM.ManagerId);
                if (!String.IsNullOrEmpty(employeeCreationVM.ManagerId))
                    employeeCreationVM.Manager = _Mapper.Map<EmployeeCreationVM>(await _EmployeeRepository.FindByIdAsync(employeeCreationVM.ManagerId));

            }
            //User edited inside the company
            else if ((currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager | UserRoles.Employee)) > 0) {
                bool readOnly = currentUserRoles == UserRoles.Employee;
                employeeCreationVM.ManagerId = currentEmployee.Id;
                employeeCreationVM.Managers = (await _EmployeeRepository.WhereAsync(x => x.CompanyId == currentEmployee.CompanyId))
                    .Select(x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, x.Id.Equals(currentEmployee.Id))).ToList();
                employeeCreationVM.Manager = _Mapper.Map<EmployeeCreationVM>(currentEmployee);
                employeeCreationVM.ManagerEnabled = !readOnly;
                employeeCreationVM.CompanyEnabled = false;
                employeeCreationVM.CompanyId = (int)currentEmployee.CompanyId;
                employeeCreationVM.Company = currentEmployee.Company;
                employeeCreationVM.Companies = new SelectListItem[] {new SelectListItem(currentEmployee.Company.CompanyName,
                    ((int)currentEmployee.CompanyId).ToString(), true) };
            }
            return employeeCreationVM;
        }

        private async Task<bool> ValidateEmployeeCreationVMState(EmployeeCreationVM employeeCreationVM, UserRoles currentUserRoles) {
            bool modelValid = true;
            if ((currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator) {
                employeeCreationVM.ManagerEnabled = false;
                employeeCreationVM.CompanyEnabled = true;
                if (employeeCreationVM.CompanyId <= 0) {
                    modelValid = false;
                    ModelState.AddModelError("", _DataLocalizer["As app administrator you must assign company"]);
                }
            }
            //Company managers can assign Manager from the company
            else {
                employeeCreationVM.ManagerEnabled = true;
                employeeCreationVM.CompanyEnabled = false;

                //If manager is specified, his company must be the same that employee's
                if (!String.IsNullOrWhiteSpace(employeeCreationVM.ManagerId)) {
                    var manager = await _EmployeeRepository.FindByIdAsync(employeeCreationVM.ManagerId);
                    if (manager.CompanyId != employeeCreationVM.CompanyId) {
                        ModelState.AddModelError("", _DataLocalizer["Manager must be from same company"]);
                        modelValid = false;
                    }
                }
            }
            return modelValid;
        }

        [HttpPost]
        public async Task<IEnumerable<SelectListItem>> GetEmployeesByCompany(int companyId, string managerId) {
            if (String.IsNullOrWhiteSpace(managerId))
                managerId = String.Empty;
            var employees = await _EmployeeRepository.WhereAsync(empl => empl.CompanyId == companyId);
            IEnumerable<SelectListItem> result = Array.Empty<SelectListItem>();
            if (employees.Count > 0)
                result = employees.Select(x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, managerId.Equals(x.Id)));
            return result;
        }


        #endregion

        #region Updating employees

        protected class EditionPermissions {
            public bool AllowEdition;
            public bool AllowEditAccount;
            public bool AllowEditProfile;
            public bool AllowEditContacts;
            public bool AllowEditAccess;
            public bool AllowChangeManager;
            public bool AllowChangeCompany;
        }


        private async Task<EditionPermissions> GetEditionPermission(IdentityUser currentUser, Employee currentEmployee,
            Employee concernedEmployee, UserRoles currentUserRoles) {
            ///User allowed edit account in th case when
            /// - Is not employee but company admin
            /// - Is redactor is privileged user from same company
            /// - If recactor ant concerned user is same persone
            bool allowEdition = (currentUser != null && (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                ||
                (currentEmployee != null && currentEmployee.CompanyId == concernedEmployee?.CompanyId && ((currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0))
                || currentEmployee != null && currentEmployee.Id == concernedEmployee.Id;
            if (currentUser == null || !allowEdition)
                return new EditionPermissions() { AllowEdition = false };
            bool isItSelfEdition = concernedEmployee.Id.Equals(currentUser.Id);
            bool isItSameCompanyEdition = concernedEmployee.CompanyId == currentEmployee?.CompanyId;
            bool isCompanyPriveleged = (currentUserRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0;
            bool isAppAdministrator = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;


            bool allowEditProfile = (isCompanyPriveleged & isItSameCompanyEdition) || isAppAdministrator;
            bool allowEditAccess = await GetAllowedRolesAsync() != UserRoles.None;
            bool allowEditContact = (currentUserRoles >= UserRoles.HRManager) || isItSelfEdition;
            bool allowChangeManager = isCompanyPriveleged || isAppAdministrator;
            bool allowChangeCompany = (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator;

            return new EditionPermissions() {
                AllowEdition = allowEdition,
                AllowEditAccess = allowEditAccess,
                AllowEditProfile = allowEditProfile,
                AllowEditContacts = allowEditContact,
                AllowChangeManager = allowChangeManager,
                AllowChangeCompany = allowChangeCompany
            };
        }

        private bool ValidateProfileEdition(Employee originalEmployee, EmployeeCreationVM employeeVM, bool allow) {
            bool valid = true;
            valid &= allow || (originalEmployee.Title?.Equals(employeeVM.Title) ?? false);
            valid &= allow || (originalEmployee.FirstName?.Equals(employeeVM.FirstName) ?? false);
            valid &= allow || (originalEmployee.LastName?.Equals(employeeVM.LastName) ?? false);
            valid &= allow || (originalEmployee.TaxRate?.Equals(employeeVM.TaxRate) ?? false);
            valid &= allow || (originalEmployee.DateOfBirth.Equals(employeeVM.DateOfBirth));
            return valid;
        }

        private bool ValidateCompanyData(Employee employee, EmployeeCreationVM employeeVM, bool allow) {
            return allow || employee.CompanyId == employeeVM.CompanyId;
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
            bool validRoles = true;
            UserRoles employeeRoles = await _UserManager.GetUserRolesAsync(employee);
            UserRoles employeeVmRoles = LeaveManagementExtensions.ToUserRoles(employeeVM.Roles);
            validRoles = allow || (employeeRoles == employeeVmRoles);
            if (validRoles)
                validRoles |= allow && ((await ValidateAssignedRoles(employeeVmRoles)) == employeeVmRoles);
            return validRoles;
        }

        private async Task<bool> ValidateInput(Employee employee, EmployeeCreationVM employeeVM, EditionPermissions permission) {
            if (employee == null || !permission.AllowEdition)
                return false;
            bool inputValid = permission.AllowEdition;
            bool profileValid = ValidateProfileEdition(employee, employeeVM, permission.AllowEditProfile);
            if (!profileValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit profile data"]);
            inputValid &= profileValid;
            bool contactsValid = ValidateContactData(employee, employeeVM, permission.AllowEditContacts);
            if (!contactsValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit contacts data"]);
            inputValid &= contactsValid;
            bool managerValid = ValidateManagerData(employee, employeeVM, permission.AllowChangeManager);
            if (!managerValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change your manager"]);
            inputValid &= managerValid;
            bool companyValid = ValidateCompanyData(employee, employeeVM, permission.AllowChangeCompany);
            if (!companyValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change your manager"]);
            inputValid &= companyValid;
            bool rolesListValid = await ValidateRoles(employee, employeeVM, permission.AllowEditAccess);
            if (!rolesListValid)
                ModelState.AddModelError("", _DataLocalizer["You not autorized to change the roles"]);
            inputValid &= rolesListValid;
            return inputValid;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(string employeeId) {
            UserRoles currentUserRoles = UserRoles.None;
            var currentUser = await _UserManager.GetUserAsync(User);
            if (string.IsNullOrWhiteSpace(employeeId))
                employeeId = currentUser.Id;
            var currentEmployee = await _EmployeeRepository.FindByIdAsync(currentUser.Id);
            var consernedUser = await _UserManager.FindByIdAsync(employeeId);
            var consernedEmployee = await _EmployeeRepository.FindByIdAsync(employeeId);

            if (currentUser == null)
                return Forbid();
            if (consernedUser == null)
                return NotFound();
            currentUserRoles = await _UserManager.GetUserRolesAsync(currentUser);
            if (consernedUser != null && consernedEmployee == null && (currentUserRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator)
                return await UpdateIdentityUser(consernedUser);

            var editionPermission = await GetEditionPermission(currentUser, currentEmployee, consernedEmployee, currentUserRoles);
            if (!editionPermission.AllowEdition)
                return Forbid();
            var employeeRoles = await _UserManager.GetUserRolesAsync(consernedUser);
            var editionViewModel = _Mapper.Map<EmployeeCreationVM>(consernedEmployee);
            editionViewModel.RolesList = await GetListAllowedRoles(employeeRoles);
            editionViewModel.ManagerEnabled = editionPermission.AllowChangeManager;
            editionViewModel.CompanyEnabled = editionPermission.AllowChangeCompany;
            @ViewBag.Action = nameof(UpdateEmployee);
            return View(EditEmployeeView, editionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(EmployeeCreationVM employeeCreationVM) {
            var employee = await _EmployeeRepository.FindByIdAsync(employeeCreationVM.Id);
            var currentUser = await _UserManager.GetUserAsync(User);
            var currentRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var allowedRolesForAsignement = await GetAllowedRolesAsync();
            var assignedRoles = LeaveManagementExtensions.ToUserRoles(employeeCreationVM.Roles);
            var currentEmployee = await _EmployeeRepository.FindByIdAsync(currentUser.Id);
            bool reviewUserRoles = currentRoles > UserRoles.Employee;
            bool canEditUser = reviewUserRoles || employeeCreationVM.Id.Equals(currentUser.Id);
            if (!canEditUser) {
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit this profile"]);
            }
            var permissions = await GetEditionPermission(currentUser, currentEmployee, employee, currentRoles);
            bool validation = await ValidateInput(employee, employeeCreationVM, permissions);
            if (ModelState.ErrorCount == 0 && validation) {
                employee = _Mapper.Map<Employee>(employeeCreationVM);
                bool result = await _EmployeeRepository.UpdateAsync(employee);
                if (!result) {
                    ModelState.AddModelError("", _DataLocalizer["Unable to save user to repository"]);
                }
                else {
                    result &= await UpdateUserRoles(employee, assignedRoles);
                }
                if (currentRoles <= UserRoles.Employee)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index");
            }
            else {

                @ViewBag.Action = nameof(UpdateEmployee);
                employeeCreationVM = await SetEmployeeCreationVMState(employeeCreationVM, currentRoles, currentEmployee);
                return View(employeeCreationVM);
            }
        }

        private async Task<bool> UpdateUserRoles(IdentityUser employee, UserRoles assignedRoles) {
            bool updateResult = true;
            //TODO make update mechanics
            UserRoles originalRoles = await _UserManager.GetUserRolesAsync(employee);
            return updateResult;
        }

        public async Task<IActionResult> UpdateIdentityUser(IdentityUser user) {
            return await Task.FromResult(RedirectToPage($"/Account/Manage/Index"));
        }

        #endregion

        #region Index
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
            var currentEmployee = await _EmployeeRepository.FindByIdAsync(currentUser.Id);
            ///Company admin can browse all
            if ((currentEmployeeRoles & UserRoles.AppAdministrator) == UserRoles.AppAdministrator) {
                if (companyId != null)
                    selectedEmployees = (await _EmployeeRepository.WhereAsync(x => x.CompanyId == (int)companyId)).ToList();
                else {
                    selectedEmployees = (await _EmployeeRepository.WhereAsync(x => true)).ToList();
                }
            }
            ///If user is not member of app administrator, but priveleged inside the company, he can still see the users from his company
            else if (currentEmployee != null && ((currentEmployeeRoles & (UserRoles.CompanyAdministrator | UserRoles.HRManager)) > 0)) {
                selectedEmployees = (await _EmployeeRepository.WhereAsync(x => x.CompanyId == currentEmployee.CompanyId)).ToList();
            }
            else
                return Forbid();
            var employeesList = new List<EmployeePresentationDefaultViewModel>();
            foreach (Employee employee in selectedEmployees) {
                var employeePresentation = _Mapper.Map<EmployeePresentationDefaultViewModel>(employee);
                employeePresentation.CanAllocateLeave = employee.CompanyId == currentEmployee.CompanyId;
                employeesList.Add(employeePresentation);
            }
            return View(employeesList);
        }


        #endregion

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
                    \tYour user name is: {1}<br/>
                    \tpassword: {2}<br/>
                Have a nice day and see you later!";
            string letterText = _DataLocalizer[model, newEmployee.FormatEmployeeSNT(), newEmployee.Email, password];
            string subject = _DataLocalizer["Your account at LeaveManagement System was created"];
            string contactMail = String.IsNullOrWhiteSpace(newEmployee.ContactMail) ? newEmployee.Email : newEmployee.ContactMail;
            await _EmailSender.SendEmailAsync(contactMail, subject, letterText);
        }
        #endregion

    }
}

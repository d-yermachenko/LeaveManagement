using AutoMapper;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Controllers {
    public class EmployeeController : Controller {

        private const string RegisterEmployeeView = "RegisterEmployee2Col";

        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly ILogger<EmployeePresentationDefaultViewModel> _Logger;
        private readonly IEmailSender _EmailSender;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly IStringLocalizer _DataLocalizer;
        private readonly IMapper _Mapper;

        public EmployeeController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EmployeePresentationDefaultViewModel> logger,
            IEmailSender emailSender,
            IEmployeeRepositoryAsync employeeRepository,
            IStringLocalizerFactory localizerFactory,
            IMapper mapper) {
            _SignInManager = signInManager;
            _UserManager = userManager;
            _RoleManager = roleManager;
            _Logger = logger;
            _EmailSender = emailSender;
            _EmployeeRepository = employeeRepository;
            _DataLocalizer = localizerFactory.Create(typeof(Areas.Identity.Pages.Account.RegisterModel));
            _Mapper = mapper;
        }

        string ReturnUrl = "";

        public async Task<UserRoles> GetAllowedRolesAsync() {
            UserRoles userMemberShip = await _UserManager.GetUserRolesAsync(User);
            UserRoles result = UserRoles.None;
            if ((userMemberShip & UserRoles.Administrator) == UserRoles.Administrator)
                result |= UserRoles.LocalAdministrator;
            if ((userMemberShip & UserRoles.LocalAdministrator) == UserRoles.LocalAdministrator)
                result |= (UserRoles.LocalAdministrator | UserRoles.Employee | UserRoles.HRManager);
            if ((userMemberShip & UserRoles.HRManager) == UserRoles.HRManager)
                result |= UserRoles.Employee;

            return result;
        }

        public async Task<IEnumerable<SelectListItem>> GetListAllowedRoles(UserRoles assignedRoles = UserRoles.None) {
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

        [HttpGet]
        public async Task<IActionResult> CreateEmployee() {
            ReturnUrl = HttpContext.Request.Headers["Referer"];
            if (!await _UserManager.IsPrivelegedUser(User)) {
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
            ViewBag.Action = nameof(CreateEmployee);
            return View(RegisterEmployeeView, employeeCreationVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeCreationVM employeeCreationVM) {
            string referer = employeeCreationVM.ReturnUrl;
            var currentUser = await _UserManager.GetUserAsync(User);
            if (!await _UserManager.IsPrivelegedUser(currentUser)) {
                ModelState.AddModelError("", _DataLocalizer["Action permitted only to administrators"]);
                return Forbid();
            }

            if ((await _UserManager.FindByEmailAsync(employeeCreationVM.Email)) != null) {
                ModelState.AddModelError("", _DataLocalizer["User with same email already exists"]);
            }
            UserRoles employeeRoles = LeaveManagementExtensions.ToUserRoles(employeeCreationVM.Roles);
            UserRoles allowedRoles = await GetAllowedRolesAsync();
            if ((employeeRoles ^ allowedRoles) > 0)
                _Logger.LogWarning($"Attempting assign of disabled roles for user {currentUser.UserName}");
            employeeRoles = employeeRoles & allowedRoles; // Removing not allowed roles;

            if (employeeCreationVM.AcceptContract != true) {
                ModelState.AddModelError("", _DataLocalizer["You must accept the contract"]);
            }
            if (ModelState.ErrorCount > 0) {
                employeeCreationVM.RolesList = await GetListAllowedRoles(employeeRoles);
            }
            if (ModelState.ErrorCount == 0) {
                
                Employee employee = _Mapper.Map<Employee>(employeeCreationVM);
                employee.DisplayName = employee.FormatEmployeeSNT();
                employee.ManagerId = currentUser.Id;
                bool result = await _EmployeeRepository.RegisterEmployeeAsync(employee, employeeCreationVM.Password);
                if (!result)
                    ModelState.AddModelError("", _DataLocalizer["Unabled to save employee into repository"]);
            }
            if (ModelState.ErrorCount == 0) {
                var employeeIdentityUser = await _UserManager.FindByEmailAsync(employeeCreationVM.Email);
                LeaveManagementExtensions.FromUserRoles(employeeRoles).ToList().ForEach(async role => {
                    var identityResult = await _UserManager.AddToRoleAsync(employeeIdentityUser, role);
                    if (!identityResult.Succeeded)
                        ModelState.AddModelError("", _DataLocalizer["Unable to add this user to role {0}", role]);
                });
            }
            if(ModelState.ErrorCount > 0) {
                employeeCreationVM.RolesList = await GetListAllowedRoles();
                ViewData["Referer"] = referer;
                ViewBag.Action = nameof(CreateEmployee);
                return View(RegisterEmployeeView, employeeCreationVM);
            }
            else {
                return Redirect(referer);
            }

        }

        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(string employeeId) {
            var currentUser = await _UserManager.GetUserAsync(User);
            if (!await _UserManager.IsPrivelegedUser(currentUser)) {
                ModelState.AddModelError("", _DataLocalizer["Action permitted only to administrators"]);
                return Forbid();
            }
            var employeeToEdit = await _EmployeeRepository.FindByIdAsync(employeeId);
            if (employeeToEdit == null)
                return NotFound(_DataLocalizer["User not found"]);
            var employeeRoles = await _UserManager.GetRolesAsync(employeeToEdit);
            var editionViewModel = _Mapper.Map<EmployeeCreationVM>(employeeToEdit);
            editionViewModel.RolesList = await GetListAllowedRoles(LeaveManagementExtensions.ToUserRoles(employeeRoles));
            return View(RegisterEmployeeView, editionViewModel);
        }


        public async Task<IActionResult> UpdateEmployee(EmployeeCreationVM employeeCreationVM) {
            var employee = await _EmployeeRepository.FindByIdAsync(employeeCreationVM.Id);
            var currentUser = await _UserManager.GetUserAsync(User);
            var currentRoles = await _UserManager.GetUserRolesAsync(currentUser);
            var allowedRolesForAsignement = await GetAllowedRolesAsync();
            var assignedRoles = LeaveManagementExtensions.ToUserRoles(employeeCreationVM.Roles);
            bool reviewUserRoles = currentRoles > UserRoles.Employee;
            bool canEditUser = reviewUserRoles || employeeCreationVM.Id.Equals(currentUser.Id);
            if (!canEditUser) {
                ModelState.AddModelError("", _DataLocalizer["You not autorized to edit this profile"]);
            }
            if(ModelState.ErrorCount== 0) {
                employee = _Mapper.Map<Employee>(employeeCreationVM);
                bool result = await _EmployeeRepository.UpdateAsync(employee);
                if (!result) {
                    ModelState.AddModelError("", _DataLocalizer["Unable to save user to repository"]);
                }
            }
            assignedRoles = assignedRoles & allowedRolesForAsignement;

            return Forbid();
        }
    }
}

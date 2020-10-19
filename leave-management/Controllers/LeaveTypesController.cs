using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Controllers {

    [MiddlewareFilter(typeof(LocalizationPipeline))]
    [Authorize]
    public class LeaveTypesController : Controller {

        private readonly ILogger<LeaveTypesController> _Logger;

        private readonly Contracts.ILeaveTypeRepositoryAsync _LeaveTypesRepository;

        private readonly AutoMapper.IMapper _Mapper;

        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;

        private readonly IStringLocalizer _ControllerLocalizer;

        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;


        public LeaveTypesController(Contracts.ILeaveTypeRepositoryAsync repository,
            AutoMapper.IMapper mapper,
            ILogger<LeaveTypesController> logger,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            SignInManager<IdentityUser> signInManager,
            IEmployeeRepositoryAsync employees) {
            _LeaveTypesRepository = repository;
            _Mapper = mapper;
            _Logger = logger;
            _EmployeeRepository = employees;
            _LocalizerFactory = localizerFactory;
            _ControllerLocalizer = localizerFactory.Create(typeof(LeaveTypesController));
            _SignInManager = signInManager;
        }


        private async Task<IEnumerable<LeaveType>> GetListLeaveTypesForThisCompany() {
            int? currentEmployeesCompanyId = await GetCurrentEmployeesCompany();
            if (currentEmployeesCompanyId == null)
                return Array.Empty<LeaveType>();
            else
                return await _LeaveTypesRepository.WhereAsync(lt => lt.CompanyId == currentEmployeesCompanyId);
        }

        private async Task<int?> GetCurrentEmployeesCompany() {
            var currentUser = await _SignInManager.UserManager.GetUserAsync(User);
            var currentEmployee = await _EmployeeRepository.FindByIdAsync(currentUser.Id);
            return currentEmployee?.CompanyId;
        }

        #region Reading
        // GET: LeaveTypes
        public async Task<ActionResult> Index() {
            if ((await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User)) != true)
                return Forbid();
            List<LeaveType> leaveTypes = (await GetListLeaveTypesForThisCompany()).ToList();
            var viewModel = _Mapper.Map<List<LeaveType>, List<LeaveTypeNavigationViewModel>>(leaveTypes);
            return View(viewModel);
        }

        // GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int id) {
            if ((await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User)) != true)
                return Forbid();
            var entry = await _LeaveTypesRepository.FindByIdAsync(id);
            if (entry == null) {
                string errorMessage = _ControllerLocalizer["Impossible to find leave type #{0}. Please check the adress", id];
                return StatusCode(StatusCodes.Status404NotFound, errorMessage);
            }
            var viewModel = _Mapper.Map<Data.Entities.LeaveType, LeaveTypeNavigationViewModel>(entry);
            return View(viewModel);
        }
        #endregion

        #region Create

        // GET: LeaveTypes/Create
        public async Task<ActionResult> Create() {
            if ((await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User)) != true)
                return Forbid();
            return View();
        }


        //POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeEditionViewModel creationModel) {
            int? currentEmplCompanyId = await GetCurrentEmployeesCompany();
            if (!(await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User)) || currentEmplCompanyId == null)
                return Forbid();
            string newName = string.Empty;
            if (!ModelState.IsValid) {
                HomeController.DisplayProblem(_Logger, this, _ControllerLocalizer["Model state invalid"],
                    _ControllerLocalizer[ModelState.ValidationState.ToString()]);
                return View(creationModel);
            }
            newName = creationModel.LeaveTypeName;
            var leaveTypes = (await GetListLeaveTypesForThisCompany()).Where(x => x.LeaveTypeName?.Equals(newName) ?? false);
            if (leaveTypes?.Count() > 0) {
                ModelState.AddModelError("", _ControllerLocalizer["Leave type with name {0} already exists", newName]);
                return View(creationModel);
            }
            var createdLeaveType = _Mapper.Map<LeaveType>(creationModel);
            createdLeaveType.DateCreated = DateTime.Now;
            createdLeaveType.CompanyId = (int)currentEmplCompanyId;
            if (!(await _LeaveTypesRepository.CreateAsync(createdLeaveType))) {
                ModelState.AddModelError("", _ControllerLocalizer["Failed to save leave type '{0}' in repositary", newName]);
                return View(creationModel);
            };
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit
        // GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int id) {
            if (!(await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User))) {
                ModelState.AddModelError("401", _ControllerLocalizer["You are not in role authorized to edit leave types"]);
                return Forbid();
            }

            try {
                var leaveType = await _LeaveTypesRepository.FindByIdAsync(id);
                if (leaveType == null) {
                    ModelState.AddModelError("401", _ControllerLocalizer["Leave type not found"]);
                    return NotFound();
                }
                if (leaveType.CompanyId != await GetCurrentEmployeesCompany()) {
                    ModelState.AddModelError("401", _ControllerLocalizer["You are not authorized to edit this leave type"]);
                    return Forbid();
                }
                var leaveTypeViewModel = _Mapper.Map<LeaveType, LeaveTypeEditionViewModel>(leaveType);
                return View(leaveTypeViewModel);
            }
            catch (DbException error) {
                HomeController.DisplayProblem(_Logger, this, _ControllerLocalizer[error.ToString()], error.Message);
                return View();
            }
            catch (Exception error) {
                _Logger.LogError(new EventId(error.HResult, error.ToString()), error.Message);
                ViewBag.Message = _ControllerLocalizer[error.ToString()] + "\n" + error.Message;
                return View();
            }

        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, LeaveTypeEditionViewModel editionViewModel) {
            Employee currentEmployee = await _EmployeeRepository.GetEmployeeAsync(User);
            if (currentEmployee == null || !(await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User))) {
                ModelState.AddModelError("401", _ControllerLocalizer["You are not in role authorized to edit leave types"]);
                return Forbid();
            }
            string newName = string.Empty;
            if (!ModelState.IsValid) {
                return View();
            }
            var leaveTypeToChange = await _LeaveTypesRepository.FindByIdAsync(id);
            if (leaveTypeToChange == null) {
                ModelState.AddModelError("404", _ControllerLocalizer["Leave type not found"]);
                return NotFound();
            }
            if (leaveTypeToChange.CompanyId != currentEmployee.CompanyId) {
                ModelState.AddModelError("401", _ControllerLocalizer["You forbidden to edit leave types from other company"]);
                return Forbid();
            }
            newName = editionViewModel.LeaveTypeName;
            var leaveTypesWithSameNames = _LeaveTypesRepository.WhereAsync(x => x.LeaveTypeName?.Equals(newName) ?? false &&
            x.Id != id && x.CompanyId == currentEmployee.CompanyId);
            leaveTypeToChange.LeaveTypeName = newName;
            leaveTypeToChange.DefaultDays = editionViewModel.DefaultDays;
            if (!(await _LeaveTypesRepository.UpdateAsync(leaveTypeToChange))) {
                string errorMessage = _ControllerLocalizer["Failed to rename this leave type to {0}", newName];
                ModelState.AddModelError("UpdateFailed", errorMessage);
                return View(editionViewModel);
            }
            else
                return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Remove
        // GET: LeaveTypes/Delete/5
        public async Task<ActionResult> Delete(int id) {
            Employee currentEmployee = await _EmployeeRepository.GetEmployeeAsync(User);
            var instanceToDelete = await _LeaveTypesRepository.FindByIdAsync(id);
            if (instanceToDelete == null) {
                string errorMessage = _ControllerLocalizer["Leave type with number {0} was not found", id];
                ModelState.AddModelError("", errorMessage);
                return View(nameof(Index));
            }
            bool proceedRemoving = ((await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User)) && instanceToDelete.CompanyId == currentEmployee.CompanyId)
                || await _SignInManager.UserManager.IsMemberOfOneAsync(currentEmployee, UserRoles.AppAdministrator);
            if (!proceedRemoving) {
                string errorMessage = _ControllerLocalizer["You have not permission to remove leave type"];
                ModelState.AddModelError("", errorMessage);
                return Forbid();
            }
            var leaveTypeViewModel = _Mapper.Map<LeaveTypeNavigationViewModel>(instanceToDelete);
            bool isSucceed = await _LeaveTypesRepository.DeleteAsync(instanceToDelete);
            if (!isSucceed)
                ModelState.AddModelError("", _ControllerLocalizer["Can't delete leave type with id {0}", id]);
            return View(nameof(Index));

        }
        #endregion
    }
}
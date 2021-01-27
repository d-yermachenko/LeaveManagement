using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.Repository.Entity;
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

        private readonly ILeaveManagementUnitOfWork _UnitOfWork;

        private readonly AutoMapper.IMapper _Mapper;

        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;

        private readonly IStringLocalizer _ControllerLocalizer;

        private readonly SignInManager<IdentityUser> _SignInManager;



        public LeaveTypesController(ILeaveManagementUnitOfWork unitOfWork,
            AutoMapper.IMapper mapper,
            ILogger<LeaveTypesController> logger,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            SignInManager<IdentityUser> signInManager) {
            _UnitOfWork = unitOfWork;
            _Mapper = mapper;
            _Logger = logger;
            _LocalizerFactory = localizerFactory;
            _ControllerLocalizer = localizerFactory.Create(typeof(LeaveTypesController));
            _SignInManager = signInManager;
        }


        private async Task<IEnumerable<LeaveType>> GetListLeaveTypesForThisCompany() {
            int? currentEmployeesCompanyId = await GetCurrentEmployeesCompany();
            if (currentEmployeesCompanyId == null)
                return Array.Empty<LeaveType>();
            else
                return await _UnitOfWork.LeaveTypes.WhereAsync(lt => lt.CompanyId == currentEmployeesCompanyId,
                    order: o=>o.OrderBy(lt=>lt.LeaveTypeName),
                    includes: new System.Linq.Expressions.Expression<Func<LeaveType, object>>[] { x => x.Company } );
        }

        private async Task<int?> GetCurrentEmployeesCompany() {
            var currentUser = await _SignInManager.UserManager.GetUserAsync(User);
            var currentEmployee = await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(currentUser.Id));
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
            var entry = await _UnitOfWork.LeaveTypes.FindAsync(x=>x.Id ==id,
                includes: new System.Linq.Expressions.Expression<Func<LeaveType, object>>[] { x => x.Company});
            if (entry == null) {
                string errorMessage = _ControllerLocalizer["Impossible to find leave type #{0}. Please check the address", id];
                ModelState.AddModelError("", errorMessage);
                return NotFound(errorMessage);
            }
            var viewModel = _Mapper.Map<LeaveType, LeaveTypeNavigationViewModel>(entry);
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
            bool creationResult = await _UnitOfWork.LeaveTypes.CreateAsync(createdLeaveType);
            creationResult &= await _UnitOfWork.Save();
            if (!creationResult) {
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
                var leaveType = await _UnitOfWork.LeaveTypes.FindAsync(x=>x.Id == id);
                if (leaveType == null) {
                    ModelState.AddModelError("404", _ControllerLocalizer["Leave type not found"]);
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
            Employee currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null || !(await _SignInManager.UserManager?.IsCompanyPrivelegedUser(User))) {
                ModelState.AddModelError("401", _ControllerLocalizer["You are not in role authorized to edit leave types"]);
                return Forbid();
            }
            string newName = string.Empty;
            if (!ModelState.IsValid) {
                return View();
            }
            var leaveTypeToChange = await _UnitOfWork.LeaveTypes.FindAsync(x=>x.Id == id);
            if (leaveTypeToChange == null) {
                ModelState.AddModelError("404", _ControllerLocalizer["Leave type not found"]);
                return NotFound();
            }
            if (leaveTypeToChange.CompanyId != currentEmployee.CompanyId) {
                ModelState.AddModelError("401", _ControllerLocalizer["You forbidden to edit leave types from other company"]);
                return Forbid();
            }
            newName = editionViewModel.LeaveTypeName;
            var leaveTypesWithSameNames = _UnitOfWork.LeaveTypes.WhereAsync(x => x.LeaveTypeName != null && x.LeaveTypeName.Equals(newName)  &&
            x.Id != id && x.CompanyId == currentEmployee.CompanyId);
            leaveTypeToChange.LeaveTypeName = newName;
            leaveTypeToChange.DefaultDays = editionViewModel.DefaultDays;
            bool editResult = await _UnitOfWork.LeaveTypes.UpdateAsync(leaveTypeToChange);
            editResult &= await _UnitOfWork.Save();
            if (!editResult) {
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
            Employee currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            var instanceToDelete = await _UnitOfWork.LeaveTypes.FindAsync(x=>x.Id == id);
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
            bool isSucceed = await _UnitOfWork.LeaveTypes.DeleteAsync(instanceToDelete);
            isSucceed &= await _UnitOfWork.Save();
            if (!isSucceed)
                ModelState.AddModelError("", _ControllerLocalizer["Can't delete leave type with id {0}", id]);
            return View(nameof(Index));

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
﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.Repository.Entity;
using LeaveManagement.ViewModels.Employee;
using LeaveManagement.ViewModels.LeaveAllocation;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using LeaveManagement.Code;
using Org.BouncyCastle.Utilities.IO;
using System.Linq.Expressions;

namespace LeaveManagement.Controllers {
    [Authorize]
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class LeaveAllocationController : Controller {
        #region Constructor and depencies
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly ILogger<LeaveAllocationController> _Logger;
        private readonly IMapper _Mapper;
        private Employee _CurrentEmployee;
        private const string CreateLeaveAllocationView = "CreateLeaveAllocation";
        private readonly IStringLocalizer _Localizer;

        public LeaveAllocationController(
            ILeaveManagementUnitOfWork unitOfWork,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            UserManager<IdentityUser> userManager,
            ILogger<LeaveAllocationController> logger,
            IMapper mapper) : base() {
            _LocalizerFactory = localizerFactory;
            _UnitOfWork = unitOfWork;
            _UserManager = userManager;
            _Logger = logger;
            _Mapper = mapper;
            _Localizer = _LocalizerFactory.Create(this.GetType());
        }
        #endregion

        #region Create allocation by leaveType
        [HttpGet]
        public async Task<ActionResult> AllocateByLeaveTypes() {
            if (!await _UserManager.IsCompanyPrivelegedUser(User))
                return Forbid();
            var currentEmployee = await GetCurrentEmployeeAsync();
            var avalableLeaveTypes = await _UnitOfWork.LeaveTypes.WhereAsync(
                filter: x => currentEmployee.CompanyId == x.CompanyId,
                order: o => o.OrderBy(x => x.LeaveTypeName),
                includes: new System.Linq.Expressions.Expression<Func<LeaveType, object>>[] { x => x.Company }
                );
            LeaveAllocationLeaveTypesListViewModel allocateLeaveViewModel = new LeaveAllocationLeaveTypesListViewModel() {
                AvalableLeaveTypes = _Mapper.Map<List<LeaveTypeNavigationViewModel>>(avalableLeaveTypes.ToList())
            };
            return View(allocateLeaveViewModel);
        }

        [HttpGet]
        public async Task<ActionResult> CreateByLeaveType(int leaveType) {
            var currentEmployee = await GetCurrentEmployeeAsync();
            if (!await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _Localizer["You're forbidden to create allocation"]);
                Forbid();
            }
            var leaveTypeObj = await _UnitOfWork.LeaveTypes.FindAsync(x => x.Id == leaveType);
            if (currentEmployee.CompanyId != leaveTypeObj.CompanyId) {
                ModelState.AddModelError("", _Localizer["You're forbidden to create allocation because it belongs to other company"]);
                return Forbid();
            }
            LeaveAllocationEditionViewModel allocationEditionViewModel = await InitializeLeaveAllocationEditionViewModel(
            leaveType: leaveTypeObj,
            includeEmployeesDictionary: true,
            includeLeaveTypesDictionary: true,
            allowEditPeriod: true);
            SetEditViewProperties(allowEditPeriod: true, allowEditEmployee: true);
            ViewBag.Action = nameof(CreateNewByLeaveType);
            return View(CreateLeaveAllocationView, allocationEditionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNewByLeaveType(LeaveAllocationEditionViewModel leave) {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (!await _UserManager.IsCompanyPrivelegedUser(User)) {
                ModelState.AddModelError("", _Localizer["You're forbidden to create allocation"]);
                return Forbid();
            }
            if (!ModelState.IsValid)
                return View(leave);
            var leaveTypeObj = await _UnitOfWork.LeaveTypes.FindAsync(x => x.Id == leave.AllocationLeaveTypeId);
            var consernedEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(leave.AllocationEmployeeId));
            if (currentEmployee.CompanyId != consernedEmployee.CompanyId) {
                ModelState.AddModelError("", _Localizer["You're forbidden to create allocation because you allocate leave to employee from other company"]);
                return Forbid();
            }
            if (leaveTypeObj.CompanyId != consernedEmployee.CompanyId) {
                ModelState.AddModelError("", _Localizer["You're forbidden to create allocation because it belongs to other company"]);
                return Unauthorized();
            }
            int period = leave.Period;
            var newLeaveAllocation = new LeaveAllocation() {
                AllocationEmployeeId = leave.AllocationEmployeeId,
                AllocationLeaveTypeId = leave.AllocationLeaveTypeId,
                DateCreated = leave.DateCreated,
                NumberOfDays = leave.NumberOfDays,
                Period = period,
            };

            bool succeed = await _UnitOfWork.LeaveAllocations.CreateAsync(newLeaveAllocation);
            succeed &= await _UnitOfWork.Save();
            if (succeed) {
                return await UserLeaveAllocationsForPeriod(userId: leave.AllocationEmployeeId, period: leave.Period.ToString()); ;
            }
            else {
                leave = await InitializeLeaveAllocationEditionViewModel(
                    employee: consernedEmployee,
                    leaveType: leaveTypeObj,
                    period: leave.Period,
                    includeEmployeesDictionary: true
                    );
                SetEditViewProperties(allowEditPeriod: true, allowEditEmployee: true, allowEditLeaveType: false);
                return View(leave);
            }
        }

        /// <summary>
        /// Allocates leave to all employees for current leave type and for given period
        /// If period is empty, use current period.
        /// </summary>
        /// <param name="leaveTypeId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public async Task<ActionResult> CreateAllocationToAllEmployees(int leaveTypeId, int? period = null, int? numberOfDays = null) {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _Localizer["You're not employee - you're not authorized to allocate leave"]);
                return Forbid();
            }
            if (!await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _Localizer["You not authorized to allocate leave"]);
                return Forbid();
            }
            LeaveType leaveType = null;
            try {
                leaveType = await _UnitOfWork.LeaveTypes.FindAsync(x => x.Id == leaveTypeId);
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message, leaveTypeId);
                throw;
            }
            if (leaveType == null) {
                ModelState.AddModelError("", _Localizer["Leave type not found"]);
                return NotFound(_Localizer["Leave type not found"]);
            }
            if (leaveType.CompanyId != currentEmployee.CompanyId) {
                ModelState.AddModelError("", _Localizer["Leave type you try allocate is not leave type from your company"]);
            }
            if (period == null)
                period = DateTime.Now.Year;
            if (numberOfDays == null)
                numberOfDays = leaveType.DefaultDays;

            var employees = await _UnitOfWork.Employees.WhereAsync(filter: empl => empl.CompanyId == leaveType.CompanyId,
                order: o => o.OrderBy(x => x.LastName).ThenBy(y => y.FirstName),
                includes: Array.Empty<Expression<Func<Employee, object>>>());
            bool savingResult = true;
            foreach (var employee in employees) {
                var hasAlreadyAllocations = (await _UnitOfWork.LeaveAllocations.WhereAsync(q =>
                    q.AllocationEmployeeId.Equals(employee.Id)
                    && q.Period == period
                    && q.AllocationLeaveTypeId == leaveTypeId
                )).Any();
                if (!hasAlreadyAllocations) {
                    bool localSavingResult = await _UnitOfWork.LeaveAllocations.CreateAsync(new LeaveAllocation() {
                        AllocationEmployeeId = employee.Id,
                        AllocationLeaveTypeId = leaveTypeId,
                        Period = (int)period,
                        NumberOfDays = (int)numberOfDays
                    });
                    localSavingResult &= await _UnitOfWork.Save();
                    if (!localSavingResult) {
                        _Logger.LogError(new EventId(),
                            _Localizer["Failed to allocate leave to {0} (id: {1})", employee.FormatEmployeeSNT(), employee.Id]);
                    }
                    savingResult &= localSavingResult;
                }
            }

            if (savingResult)
                return RedirectToAction(nameof(UserLeaveAllocationsForPeriod), new { period = period.ToString(), leaveTypeId, userId = "*" });
            else {
                ModelState.AddModelError("UnableAllocateAll", _Localizer["Unable to save allocations. Information was written to log"]);
                return await AllocateByLeaveTypes();
            }
        }
        #endregion

        #region Show user allocations
        /// <summary>
        /// Shows and filteres list of leave allocations
        /// </summary>
        /// <param name="userId">User id. In the case of empty - takes current. In the case of "*" - all. In the case of value - takes specific</param>
        /// <param name="period">Period as string. In the case of empty - takes current. In the case of any not - all. In the case of number - takes specific</param>
        /// <param name="leaveTypeId">Leave type. In case of null - takes all, otherwise - specific</param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> UserLeaveAllocationsForPeriod(string userId = "", string period = "", int? leaveTypeId = null, bool showRequestButton = false) {
            var currentEmployee = await GetCurrentEmployeeAsync();
            IEnumerable<LeaveAllocation> leaveAllocations = await _UnitOfWork.LeaveAllocations.WhereAsync(al => al.AllocationEmployee != null &&
                al.AllocationEmployee.CompanyId != null && al.AllocationEmployee.CompanyId == currentEmployee.CompanyId,
                includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationEmployee, x => x.AllocationLeaveType });
            var isPreveleged = await _UserManager.IsCompanyPrivelegedUser(currentEmployee);
            //------------------------- Filtering by user.
            if (userId.Equals("*")) { // All users
                if (!isPreveleged) // if user not priveleged - restricting him to himself
                    leaveAllocations = leaveAllocations.Where(q => q.AllocationEmployeeId.Equals(currentEmployee.Id));
            }
            else if (String.IsNullOrWhiteSpace(userId)) { // Empty - current user
                leaveAllocations = leaveAllocations.Where(q => q.AllocationEmployeeId.Equals(currentEmployee.Id));
            }
            else //In other case we filter by argument
                leaveAllocations = leaveAllocations.Where(q => q.AllocationEmployeeId.Equals(currentEmployee.Id));
            //------------------------Filtering by period
            if (String.IsNullOrWhiteSpace(period)) { // If period is empty - it is current period
                leaveAllocations = leaveAllocations.Where(q => q.Period == DateTime.Now.Year);
            }
            else if (int.TryParse(period, out int periodVal)) { // If period is number - filtering by period
                leaveAllocations = leaveAllocations.Where(q => q.Period == periodVal);
            }
            else if (!period.Equals("*")) //If period is not number nor conventional value, saying that request id incorrect
                return BadRequest();
            // In the case of "*" we dont  want ro gilter the period
            //-----------------------Filter by leave type
            if (leaveTypeId != null) //If leave type passed, filtering by leaveType
                leaveAllocations = leaveAllocations.Where(q => q.AllocationLeaveTypeId == leaveTypeId);

            ViewBag.ShowRequestButton = showRequestButton;

            var allocationsLeave = _Mapper.Map<List<LeaveAllocationPresentationViewModel>>(leaveAllocations);
            return View("UserLeaveAllocationsForPeriod", allocationsLeave);
        }
        #endregion

        #region Create allocation by employee


        [HttpGet]
        public async Task<ActionResult> AllocateByEmployee(string employeeId) {
            Employee concernedEmployee = null;
            concernedEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(employeeId));
            if (concernedEmployee == null) {
                ModelState.AddModelError("", _Localizer["Employee not found"]);
                return NotFound();
            }

            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null || !await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _Localizer["You not authorized to allocate leave to employees"]);
                return Forbid();
            }
            if (string.IsNullOrWhiteSpace(employeeId)) {
                ModelState.AddModelError("", _Localizer["Employee id cant be empty"]);
                return BadRequest();
            }
            if (concernedEmployee.CompanyId != currentEmployee?.CompanyId) {
                ModelState.AddModelError("", _Localizer["You cant allocate the leave yo employee from other company"]);
                return Forbid();
            }
            ViewBag.Action = nameof(CreateNewByEmployee);
            var viewModel = await InitializeLeaveAllocationEditionViewModel(employee: concernedEmployee, period: DateTime.Now.Year, includeEmployeesDictionary: true,
                includeLeaveTypesDictionary: true,
                allowEditLeaveType: true,
                allowEditPeriod: true,
                allowEditEmployee: true);
            SetEditViewProperties(allowEditPeriod: true, allowEditLeaveType: true, allowEditEmployee: true);
            return View(CreateLeaveAllocationView, viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> CreateNewByEmployee(LeaveAllocationEditionViewModel leaveAllocationViewModel) {
            #region Verification of access
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null || !await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _Localizer["You not authorized to allocate leave to employees"]);
                return Forbid();
            }
            #endregion
            #region Verification of integrity
            if (!ModelState.IsValid) {
                return View(CreateLeaveAllocationView, leaveAllocationViewModel);
            }
            var concernedEmployee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(leaveAllocationViewModel.AllocationEmployeeId));
            var concernedLeaveType = await _UnitOfWork.LeaveTypes.FindAsync(x => x.Id.Equals(leaveAllocationViewModel.AllocationLeaveTypeId));
            bool companyIsIntegral = concernedEmployee.CompanyId == concernedLeaveType.CompanyId &&
                concernedLeaveType.CompanyId == currentEmployee.CompanyId;
            if (!companyIsIntegral) {
                ModelState.AddModelError("", _Localizer["Company not matches for all the actors"]);
                _Logger.LogError("There is mismatch in data: currentUser.companyId must match Company Ids of concerned user and leave type",
                    new object[] {
                        $"CurrentUser.CompanyId: {currentEmployee.CompanyId}",
                        $"ConcernedEmployee.CompanyId: {concernedEmployee.CompanyId}",
                        $"LeaveType.CompanyId: {concernedLeaveType.CompanyId}",
                    });
                return BadRequest();
            }
            int period = leaveAllocationViewModel.Period;
            #endregion
            var newLeaveAllocation = new LeaveAllocation() {
                AllocationEmployeeId = leaveAllocationViewModel.AllocationEmployeeId,
                AllocationLeaveTypeId = leaveAllocationViewModel.AllocationLeaveTypeId,
                DateCreated = leaveAllocationViewModel.DateCreated,
                NumberOfDays = leaveAllocationViewModel.NumberOfDays,
                Period = period,
            };
            bool succeed = await _UnitOfWork.LeaveAllocations.CreateAsync(newLeaveAllocation);
            succeed &= await _UnitOfWork.Save();
            if (succeed) {
                return await UserLeaveAllocationsForPeriod(userId: leaveAllocationViewModel.AllocationEmployeeId, period: leaveAllocationViewModel.Period.ToString());
            }
            else {
                var donnor = await InitializeLeaveAllocationEditionViewModel(includeEmployeesDictionary: true, includeLeaveTypesDictionary: true);
                leaveAllocationViewModel.Employees = donnor.Employees;
                leaveAllocationViewModel.AllocationLeaveTypes = donnor.AllocationLeaveTypes;
                return View(leaveAllocationViewModel);
            }
        }
        #endregion

        // GET: LeaveAllocation/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id) {
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _UnitOfWork.LeaveAllocations.FindAsync(x => x.Id == id,
                    includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationEmployee, x => x.AllocationLeaveType });
                if (leaveAllocation == null)
                    return NotFound(_Localizer["Leave allocation not found"]);

                if (!(await _UserManager.IsCompanyPrivelegedUser(User) || (await GetCurrentEmployeeAsync()).Id.Equals(leaveAllocation.AllocationEmployeeId)))
                    return Forbid();
                var leaveAllocationView = _Mapper.Map<LeaveAllocationPresentationViewModel>(leaveAllocation);
                return View(leaveAllocationView);
            }
            catch (AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _Localizer["Problem while getting leave allocation"], messages.ToString());
                return Redirect(Request.Headers["Referer"].ToString());
            }
            finally {
                ;
            }
        }


        #region Edition
        public async Task<ActionResult> Edit(int id) {
            var currentUser = await GetCurrentEmployeeAsync();
            if (!await _UserManager.IsCompanyPrivelegedUser(currentUser))
                return Forbid();
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _UnitOfWork.LeaveAllocations.FindAsync(predicate: x => x.Id == id,
                    includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationEmployee, x => x.AllocationLeaveType });
                if (leaveAllocation == null)
                    return NotFound(_Localizer["Leave allocation not found"]);
                if (leaveAllocation.AllocationLeaveType?.CompanyId == null)
                    return BadRequest();
                var leaveAllocationView = _Mapper.Map<LeaveAllocationEditionViewModel>(leaveAllocation);
                int companyId = leaveAllocation.AllocationLeaveType.CompanyId;
                var lists = await GetListLeaveTypesAndEmployees(companyId, leaveAllocation.AllocationEmployeeId, leaveAllocation.AllocationLeaveTypeId);
                leaveAllocationView.AllocationLeaveTypes = lists.Item1;
                leaveAllocationView.Employees = lists.Item2;
                SetEditViewProperties(allowEditEmployee: true, allowEditLeaveType: true
                    , allowEditPeriod: true);
                ViewBag.Action = nameof(Edit);
                return View(CreateLeaveAllocationView, leaveAllocationView);
            }
            catch (AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _Localizer["Problem while getting leave allocation"], messages.ToString());
                return Redirect(Request.Headers["Referer"].ToString());
            }
            finally {
                ;
            }
        }

        // POST: LeaveAllocation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, LeaveAllocationEditionViewModel editionViewModel) {
            if (!await _UserManager.IsCompanyPrivelegedUser(User))
                return Forbid();
            LeaveAllocation originalLeaveAllocation = await _UnitOfWork.LeaveAllocations.FindAsync(x => x.Id == id,
                includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationLeaveType, x => x.AllocationEmployee });
            try {
                if (originalLeaveAllocation == null)
                    return NotFound();
                #region Validation operation
                var newLeaveType = await _UnitOfWork.LeaveTypes.FindAsync(x => x.Id == editionViewModel.AllocationLeaveTypeId,
                    includes: new System.Linq.Expressions.Expression<Func<LeaveType, object>>[] { x => x.Company });
                #endregion
                LeaveAllocation newLeaveAllocation = _Mapper.Map<LeaveAllocation>(editionViewModel);
                await WriteHistory(newLeaveAllocation, originalLeaveAllocation);
                originalLeaveAllocation.AllocationEmployeeId = newLeaveAllocation.AllocationEmployeeId;
                originalLeaveAllocation.AllocationLeaveTypeId = newLeaveAllocation.AllocationLeaveTypeId;
                originalLeaveAllocation.Period = newLeaveAllocation.Period;
                originalLeaveAllocation.NumberOfDays = newLeaveAllocation.NumberOfDays;

                bool succeed = await _UnitOfWork.LeaveAllocations.UpdateAsync(originalLeaveAllocation);
                succeed &= await _UnitOfWork.Save();
                if (succeed) {
                    return await UserLeaveAllocationsForPeriod(editionViewModel.AllocationEmployeeId, period: editionViewModel.Period.ToString());
                }
                else {
                    if (newLeaveType?.CompanyId == null)
                        return BadRequest();
                    int companyId = newLeaveType.CompanyId;
                    var lists = await GetListLeaveTypesAndEmployees(companyId, newLeaveAllocation.AllocationEmployeeId, newLeaveAllocation.AllocationLeaveTypeId);
                    editionViewModel.AllocationLeaveTypes = lists.Item1;
                    editionViewModel.Employees = lists.Item2;
                    ViewBag.Action = nameof(Edit);
                    return View(CreateLeaveAllocationView, editionViewModel);
                }

            }
            catch (AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _Localizer["Problem while getting leave allocation"], messages.ToString());
                return Redirect(Request.Headers["Referer"].ToString());
            }
            finally {
                ;
            }
        }

        private async Task<Tuple<IEnumerable<SelectListItem>, IEnumerable<SelectListItem>>> GetListLeaveTypesAndEmployees(int companyId, string employeeId, int allocatedLeaveTypeId) {
            var employees = (await _UnitOfWork.Employees.WhereAsync(
                    filter: x => x.CompanyId == companyId,
                    order: x => x.OrderBy(el => el.LastName))).Select(
                       x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, x.Id.Equals(employeeId)));

            var allocationLeaveTypes = (await _UnitOfWork.LeaveTypes.WhereAsync(
                filter: x => x.CompanyId == companyId)).Select(
                   x => new SelectListItem(x.LeaveTypeName, x.Id.ToString(), x.Id.Equals(allocatedLeaveTypeId)));

            return Tuple.Create(allocationLeaveTypes, employees);
        }

        #endregion

        #region Deleting
        [HttpGet]
        public async Task<ActionResult> Delete(int id) {
            if (!await _UserManager.IsCompanyPrivelegedUser(User))
                return Forbid(_Localizer["You are not authorized to perform this action"]);
            object redirectData = null;
            ActionResult result;
            try {
                var leaveAllocation = await _UnitOfWork.LeaveAllocations.FindAsync(x => x.Id == id);
                if (leaveAllocation != null) {
                    var data = new { userId = leaveAllocation.AllocationEmployeeId, period = leaveAllocation.Period };
                    var saveResult = await _UnitOfWork.LeaveAllocations.DeleteAsync(leaveAllocation);
                    saveResult &= await _UnitOfWork.Save();
                    if (saveResult)
                        redirectData = data;
                }
            }
            catch (Exception e) {
                HomeController.DisplayProblem(logger: _Logger, this, e.GetType().Name, e.Message, e);
            }
            finally {
                if (redirectData != null)
                    result = RedirectToAction(nameof(UserLeaveAllocationsForPeriod), redirectData);
                else
                    result = RedirectToAction(nameof(UserLeaveAllocationsForPeriod));
            }
            return result;
        }
        #endregion


        #region Service operations



        /// <summary>
        /// Returns edition view model, defined by arguments
        /// </summary>
        /// <param name="id">Id of leave allocation. Null means that it is creation</param>
        /// <param name="employee">Employee. If null, it means current employee</param>
        /// <param name="leaveType">Leave type. Null means you must include the dictionary of leaveTypes</param>
        /// <param name="period">Period of leave. Ignored, if Id specified. Otherwise, in the case of creation if new leave allocation, specified period.
        /// In case if it is unspecified, period is current year</param>
        /// <param name="duration">Duration of leave. Ignored if id specified. If not, takes all avalable leave for this employee and leave type</param>
        /// <param name="includeEmployeesDictionary">Force include of employees dictionary to viewModel (Admin, HR specialist)</param>
        /// <param name="includeLeaveTypesDictionary">Force include leaveTypes dictionary to viewModel </param>
        /// <returns></returns>
        public async Task<LeaveAllocationEditionViewModel> InitializeLeaveAllocationEditionViewModel(long? id = null, Employee employee = null,
            LeaveType leaveType = null, int? period = null, int? duration = null, bool includeEmployeesDictionary = false,
            bool includeLeaveTypesDictionary = false,
            bool allowEditPeriod = false,
            bool allowEditLeaveType = false,
            bool allowEditEmployee = false) {
            int effectivePeriod = DateTime.Now.Year;
            if (period != null)
                effectivePeriod = (int)period;
            return await Task.Factory.StartNew(() => {
                Employee currentEmployee = GetCurrentEmployeeAsync().Result;
                LeaveAllocationEditionViewModel leaveAllocationVM = null;
                if (id == null) {
                    leaveAllocationVM = new LeaveAllocationEditionViewModel() {
                        DateCreated = DateTime.Now,
                        NumberOfDays = 0
                    };
                    if (leaveType == null)
                        includeLeaveTypesDictionary = true;
                    else {
                        leaveAllocationVM.AllocationLeaveTypeId = leaveType.Id;
                        leaveAllocationVM.AllocationLeaveType = _Mapper.Map<LeaveTypeNavigationViewModel>(leaveType);
                    }
                    if (employee == null)
                        employee = GetCurrentEmployeeAsync().Result;
                    leaveAllocationVM.AllocationEmployee = _Mapper.Map<EmployeePresentationDefaultViewModel>(employee);
                    leaveAllocationVM.AllocationEmployeeId = leaveAllocationVM.AllocationEmployee.Id;

                    if (duration == null && leaveType != null) {
                        leaveAllocationVM.NumberOfDays = leaveType.DefaultDays;
                    }
                    leaveAllocationVM.Period = effectivePeriod;
                }
                else {
                    leaveAllocationVM = _Mapper.Map<LeaveAllocationEditionViewModel>(_UnitOfWork.LeaveAllocations.FindAsync(x => x.Id == (long)id,
                        includes: new Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationEmployee, x => x.AllocationLeaveType }).Result);
                }

                includeEmployeesDictionary &= _UserManager.IsCompanyPrivelegedUser(User).Result;
                if (includeEmployeesDictionary) {
                    leaveAllocationVM.Employees = _UnitOfWork.Employees.WhereAsync(filter: x => currentEmployee != null && currentEmployee.CompanyId != null && x.CompanyId == currentEmployee.CompanyId,
                        includes: new Expression<Func<Employee, object>>[] { e => e.Company, e => e.Manager })
                        .Result
                        .Select(x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, leaveAllocationVM.AllocationEmployeeId?.Equals(x.Id) ?? false));
                }
                if (includeLeaveTypesDictionary) {
                    leaveAllocationVM.AllocationLeaveTypes = _UnitOfWork.LeaveTypes.WhereAsync(
                        filter: lt => currentEmployee != null && currentEmployee.CompanyId != null && lt.CompanyId == currentEmployee.CompanyId,
                        includes: new Expression<Func<LeaveType, object>>[] { e => e.Company })
                    .Result
                    .Select(x => new SelectListItem(x.LeaveTypeName, x.Id.ToString(), leaveAllocationVM.AllocationLeaveTypeId.Equals(x.Id)));
                }
                SetEditViewProperties(allowEditPeriod, allowEditLeaveType, allowEditEmployee);
                return leaveAllocationVM;
            });
        }

        private void SetEditViewProperties(bool allowEditPeriod = false,
            bool allowEditLeaveType = false,
            bool allowEditEmployee = false) {
            ViewBag.CanEditPeriod = allowEditPeriod;
            ViewBag.CanEditLeaveType = allowEditLeaveType;
            ViewBag.CanEditEmployee = allowEditEmployee;
        }

        public async Task<Employee> GetCurrentEmployeeAsync() {
            if (_CurrentEmployee == null)
                _CurrentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            return _CurrentEmployee;
        }

        public async Task WriteHistory(LeaveAllocation oldAllocation, LeaveAllocation newAllocation) {
            await Task.Factory.StartNew(() => {
                KellermanSoftware.CompareNetObjects.CompareLogic compareLogic = new KellermanSoftware.CompareNetObjects.CompareLogic {
                    Config = new KellermanSoftware.CompareNetObjects.ComparisonConfig() {
                        ComparePrivateFields = false,
                        ComparePrivateProperties = false,
                        TypesToInclude = new List<Type> { typeof(LeaveAllocation) }
                    }
                };
                var compareResult = compareLogic.Compare(oldAllocation, newAllocation);
                _Logger.LogInformation(_Localizer["Edition of leaving id"],
                    compareResult.DifferencesString);
            });
        }

        #endregion

        public async Task<IEnumerable<LeaveAllocation>> GetLeaveAllocationsAsync() {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            return await _UnitOfWork.LeaveAllocations.WhereAsync(
                filter: x => x.AllocationLeaveType.CompanyId == currentEmployee.CompanyId,
                includes: new Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationEmployee, x => x.AllocationLeaveType });
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

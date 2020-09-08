using System;
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

namespace LeaveManagement.Controllers {
    [Authorize]
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class LeaveAllocationController : Controller {
        #region Constructor and depencies

        private readonly ILeaveTypeRepositoryAsync _LeaveTypeRepository;
        private readonly ILeaveAllocationRepositoryAsync _LeaveAllocationRepository;
        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;
        private readonly ILeaveRequestsRepositoryAsync _LeaveHistoryRepository;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly ILogger<LeaveAllocationController> _Logger;
        private readonly IMapper _Mapper;
        private Employee _CurrentEmployee;
        private const string CreateLeaveAllocationView = "CreateLeaveAllocation";
        private readonly IStringLocalizer _Localizer;

        public LeaveAllocationController(
            ILeaveTypeRepositoryAsync leaveTypeRepository,
            ILeaveAllocationRepositoryAsync leaveAllocationRepository,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            ILeaveRequestsRepositoryAsync leaveHistoryRepository,
            IEmployeeRepositoryAsync employeeRepository,
            UserManager<IdentityUser> userManager,
            ILogger<LeaveAllocationController> logger,
            IMapper mapper) {
            _LeaveTypeRepository = leaveTypeRepository;
            _LeaveAllocationRepository = leaveAllocationRepository;
            _LocalizerFactory = localizerFactory;
            _LeaveHistoryRepository = leaveHistoryRepository;
            _EmployeeRepository = employeeRepository;
            _UserManager = userManager;
            _Logger = logger;
            _Mapper = mapper;
            _Localizer = _LocalizerFactory.Create(this.GetType());
        }
        #endregion

        #region Create allocation by leaveType
        [HttpGet]
        public async Task<ActionResult> AllocateByLeaveTypes() {
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            var avalableLeaveTypes = await _LeaveTypeRepository.FindAllAsync();
            LeaveAllocationLeaveTypesListViewModel allocateLeaveViewModel = new LeaveAllocationLeaveTypesListViewModel() {
                AvalableLeaveTypes = _Mapper.Map<List<LeaveTypeNavigationViewModel>>(avalableLeaveTypes.ToList())
            };
            return View(allocateLeaveViewModel);
        }

        [HttpGet]
        public async Task<ActionResult> CreateByLeaveType(int leaveType) {
            if (!await _UserManager.IsPrivelegedUser(User))
                Forbid();
            var leaveTypeObj = await _LeaveTypeRepository.FindByIdAsync(leaveType);
            LeaveAllocationEditionViewModel allocationEditionViewModel = await InitializeLeaveAllocationEditionViewModel(
                leaveType: leaveTypeObj,
                includeEmployeesDictionary: true,
                includeLeaveTypesDictionary: false,
                allowEditPeriod: true);
            SetEditViewProperties(allowEditPeriod: true, allowEditEmployee: true);
            ViewBag.Action = nameof(CreateNewByLeaveType);
            return View(CreateLeaveAllocationView, allocationEditionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNewByLeaveType(LeaveAllocationEditionViewModel leave) {
            if (!await _UserManager.IsPrivelegedUser(User))
                if (!ModelState.IsValid)
                    return View(leave);
            int period = leave.Period;
            var leaveTypeObj = await _LeaveTypeRepository.FindByIdAsync(leave.AllocationLeaveTypeId);
            var identityUser = await _EmployeeRepository.FindByIdAsync(leave.AllocationEmployeeId);
            var newLeaveAllocation = new LeaveAllocation() {
                AllocationEmployeeId = leave.AllocationEmployeeId,
                AllocationLeaveTypeId = leave.AllocationLeaveTypeId,
                DateCreated = leave.DateCreated,
                NumberOfDays = leave.NumberOfDays,
                Period = period,
            };

            bool succeed = await _LeaveAllocationRepository.CreateAsync(newLeaveAllocation);
            if (succeed) {
                return await UserLeaveAllocationsForPeriod(userId: leave.AllocationEmployeeId, period: leave.Period.ToString()); ;
            }
            else {
                leave = await InitializeLeaveAllocationEditionViewModel(
                    employee: identityUser,
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
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            LeaveType leaveType = null;
            try {
                leaveType = await _LeaveTypeRepository.FindByIdAsync(leaveTypeId);
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message, leaveTypeId);
                leaveType = null;
            }
            if (leaveType == null)
                return NotFound();
            if (period == null)
                period = DateTime.Now.Year;
            if (numberOfDays == null)
                numberOfDays = leaveType.DefaultDays;
            var employees = await _EmployeeRepository.FindAllAsync();
            bool savingResult = true;
            foreach (var employee in employees) {
                var hasAlreadyAllocations = (await _LeaveAllocationRepository.WhereAsync(q =>
                    q.AllocationEmployeeId.Equals(employee.Id)
                    && q.Period == period
                    && q.AllocationLeaveTypeId == leaveTypeId
                )).Any();
                if (!hasAlreadyAllocations) {
                    bool localSavingResult = await _LeaveAllocationRepository.CreateAsync(new LeaveAllocation() {
                        AllocationEmployeeId = employee.Id,
                        AllocationLeaveTypeId = leaveTypeId,
                        Period = (int)period,
                        NumberOfDays = (int)numberOfDays
                    });
                    if (!localSavingResult) {
                        _Logger.LogError(new EventId(),
                            _Localizer["Failed to allocate leave to {0} (id: {1})", employee.FormatEmployeeSNT(), employee.Id]);
                    }
                    savingResult &= localSavingResult;
                }
            }

            if (savingResult)
                return RedirectToAction(nameof(UserLeaveAllocationsForPeriod), new { period = period.ToString(), leaveTypeId = leaveTypeId, userId="*" });
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
            IEnumerable<LeaveAllocation> leaveAllocations = await _LeaveAllocationRepository.FindAllAsync();
            var currentEmployee = await GetCurrentEmployeeAsync();
            var isPreveleged = await _UserManager.IsPrivelegedUser(currentEmployee);
            //------------------------- Filtering by user.
            if (userId.Equals("*")) { // All users
                ///if(!isPreveleged) //But if not priveleged user asks for all users - we forbid it
                ///    return Forbid();
                if(!isPreveleged) // if user not priveleged - restricting him to himself
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
        public async Task<ActionResult> AllocateByEmployees() {
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            var employeesList = _Mapper.Map<List<EmployeePresentationDefaultViewModel>>(await _EmployeeRepository.FindAllAsync());
            return View(employeesList);
        }

        [HttpGet]
        public async Task<ActionResult> AllocateByEmployee(string employeeId) {
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest();
            Employee employee = null;
            ActionResult notFoundResult = null;
            try {
                employee = await _EmployeeRepository.FindByIdAsync(employeeId);
            }
            finally {
                if (employee == null)
                    notFoundResult = NotFound();
            }
            if (notFoundResult != null)
                return notFoundResult;
            ViewBag.Action = nameof(CreateNewByEmployee);
            var viewModel = await InitializeLeaveAllocationEditionViewModel(employee: employee, period: DateTime.Now.Year, includeEmployeesDictionary: true,
                includeLeaveTypesDictionary: true,
                allowEditLeaveType: true,
                allowEditPeriod: true,
                allowEditEmployee: true);
            SetEditViewProperties(allowEditPeriod: true, allowEditLeaveType: true, allowEditEmployee: true);
            return View(CreateLeaveAllocationView, viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> CreateNewByEmployee(LeaveAllocationEditionViewModel leaveAllocationViewModel) {
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            if (!ModelState.IsValid) {
                HomeController.DisplayProblem(_Logger, this, _Localizer["Cant' save leave allocation"],
                    _Localizer["Cant' save leave allocation"]);
                ViewBag.Action = "CreateNewByEmployee";
                return View(CreateLeaveAllocationView, leaveAllocationViewModel);
            }
            #region Verification of access
            //User can edit allocation only for himself, and cant make the leave more that allowed
            int period = leaveAllocationViewModel.Period;
            #endregion
            var newLeaveAllocation = new LeaveAllocation() {
                AllocationEmployeeId = leaveAllocationViewModel.AllocationEmployeeId,
                AllocationLeaveTypeId = leaveAllocationViewModel.AllocationLeaveTypeId,
                DateCreated = leaveAllocationViewModel.DateCreated,
                NumberOfDays = leaveAllocationViewModel.NumberOfDays,
                Period = period,
            };
            bool succeed = await _LeaveAllocationRepository.CreateAsync(newLeaveAllocation);
            if (succeed) {
                return await UserLeaveAllocationsForPeriod(userId: leaveAllocationViewModel.AllocationEmployeeId, period: leaveAllocationViewModel.Period.ToString());
            }
            else
                return View(leaveAllocationViewModel);
        }
        #endregion

        // GET: LeaveAllocation/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id) {
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if (leaveAllocation == null)
                    return NotFound();

                if (!(await _UserManager.IsPrivelegedUser(User) || (await GetCurrentEmployeeAsync()).Id.Equals(leaveAllocation.AllocationEmployeeId)))
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
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if (leaveAllocation == null)
                    return NotFound();
                var leaveAllocationView = _Mapper.Map<LeaveAllocationEditionViewModel>(leaveAllocation);
                leaveAllocationView.Employees = (await _EmployeeRepository.FindAllAsync()).Select(
                       x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, x.Id.Equals(leaveAllocation.AllocationEmployeeId)));
                leaveAllocationView.AllocationLeaveTypes = (await _LeaveTypeRepository.FindAllAsync()).Select(
                       x => new SelectListItem(x.LeaveTypeName, x.Id.ToString(), x.Id.Equals(leaveAllocation.AllocationLeaveTypeId)));
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
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            LeaveAllocation originalLeaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
            try {
                if (originalLeaveAllocation == null)
                    return NotFound();
                #region Validation operation
                var newLeaveType = await _LeaveTypeRepository.FindByIdAsync(editionViewModel.AllocationLeaveTypeId);
                #endregion
                LeaveAllocation newLeaveAllocation = _Mapper.Map<LeaveAllocation>(editionViewModel);
                await WriteHistory(newLeaveAllocation, originalLeaveAllocation);
                originalLeaveAllocation.AllocationEmployeeId = newLeaveAllocation.AllocationEmployeeId;
                originalLeaveAllocation.AllocationLeaveTypeId = newLeaveAllocation.AllocationLeaveTypeId;
                originalLeaveAllocation.Period = newLeaveAllocation.Period;
                originalLeaveAllocation.NumberOfDays = newLeaveAllocation.NumberOfDays;

                bool succeed = await _LeaveAllocationRepository.UpdateAsync(originalLeaveAllocation);
                if (succeed) {
                    return await UserLeaveAllocationsForPeriod(editionViewModel.AllocationEmployeeId, period: editionViewModel.Period.ToString());
                }
                else {
                    editionViewModel.Employees = (await _EmployeeRepository.FindAllAsync()).Select(
                       x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, x.Id.Equals(editionViewModel.AllocationEmployeeId)));
                    editionViewModel.AllocationLeaveTypes = (await _LeaveTypeRepository.FindAllAsync()).Select(
                       x => new SelectListItem(x.LeaveTypeName, x.Id.ToString(), x.Id.Equals(editionViewModel.AllocationLeaveTypeId)));
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
        #endregion

        #region Deleting
        [HttpGet]
        public async Task<ActionResult> Delete(int id) {
            if (!await _UserManager.IsPrivelegedUser(User))
                return Forbid();
            object redirectData = null;
            ActionResult result;
            try {
                var leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if (leaveAllocation != null) {
                    var data = new { userId = leaveAllocation.AllocationEmployeeId, period = leaveAllocation.Period };
                    var saveResult = await _LeaveAllocationRepository.DeleteAsync(leaveAllocation);
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
                    if (employee == null) {
                        employee = GetCurrentEmployeeAsync().Result;
                        leaveAllocationVM.AllocationEmployee = _Mapper.Map<EmployeePresentationDefaultViewModel>(employee);
                        leaveAllocationVM.AllocationEmployeeId = leaveAllocationVM.AllocationEmployee.Id;
                    }
                    if (duration == null && leaveType != null) {
                        leaveAllocationVM.NumberOfDays = leaveType.DefaultDays;
                    }
                    leaveAllocationVM.Period = effectivePeriod;
                }
                else {
                    leaveAllocationVM = _Mapper.Map<LeaveAllocationEditionViewModel>(_LeaveAllocationRepository.FindByIdAsync((long)id).Result);
                }

                includeEmployeesDictionary &= _UserManager.IsMemberOfOneAsync(GetCurrentEmployeeAsync().Result, UserRoles.Administrator| UserRoles.HRManager).Result;
                if (includeEmployeesDictionary) {
                    leaveAllocationVM.Employees = _EmployeeRepository.FindAllAsync()
                        .Result
                        .Select(x => new SelectListItem(x.FormatEmployeeSNT(), x.Id, leaveAllocationVM.AllocationEmployeeId?.Equals(x.Id) ?? false));
                }
                if (includeLeaveTypesDictionary) {
                    leaveAllocationVM.AllocationLeaveTypes = _LeaveTypeRepository.FindAllAsync()
                    .Result
                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.LeaveTypeName, x.Id.ToString(), leaveAllocationVM.AllocationLeaveTypeId.Equals(x.Id)));
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
                _CurrentEmployee = await _EmployeeRepository.FindByIdAsync(await Task.Run(() => _UserManager.GetUserId(User)));
            return _CurrentEmployee;
        }

        public async Task WriteHistory(LeaveAllocation oldAllocation, LeaveAllocation newAllocation) {
            await Task.Factory.StartNew(() => {
                KellermanSoftware.CompareNetObjects.CompareLogic compareLogic = new KellermanSoftware.CompareNetObjects.CompareLogic();
                compareLogic.Config = new KellermanSoftware.CompareNetObjects.ComparisonConfig() {
                    ComparePrivateFields = false,
                    ComparePrivateProperties = false,
                    TypesToInclude = new List<Type> { typeof(LeaveAllocation) }
                };
                var compareResult = compareLogic.Compare(oldAllocation, newAllocation);
                _Logger.LogInformation(_Localizer["Edition of leaving id"],
                    compareResult.DifferencesString);
            });
        }

        #endregion
    }
}
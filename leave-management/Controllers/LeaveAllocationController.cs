using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Controllers {
    [Authorize]
    public class LeaveAllocationController : Controller {
        #region Constructor and depencies

        private readonly ILeaveTypeRepositoryAsync _LeaveTypeRepository;
        private readonly ILeaveAllocationRepositoryAsync _LeaveAllocationRepository;
        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;
        private readonly ILeaveHistoryRepositoryAsync _LeaveHistoryRepository;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly ILogger<LeaveAllocationController> _Logger;
        private readonly IMapper _Mapper;
        private Employee _CurrentEmployee;
        private const string CreateLeaveAllocationView = "CreateLeaveAllocation";

        public LeaveAllocationController(
            ILeaveTypeRepositoryAsync leaveTypeRepository,
            ILeaveAllocationRepositoryAsync leaveAllocationRepository,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            ILeaveHistoryRepositoryAsync leaveHistoryRepository,
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
        }
        #endregion

        #region Create allocation by leaveType
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> AllocateByLeaveTypes() {
            var avalableLeaveTypes = await _LeaveTypeRepository.FindAllAsync();
            LeaveAllocationLeaveTypesListViewModel allocateLeaveViewModel = new LeaveAllocationLeaveTypesListViewModel() {
                AvalableLeaveTypes = _Mapper.Map<List<LeaveTypeNavigationViewModel>>(avalableLeaveTypes.ToList())
            };
            return View(allocateLeaveViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Employee")]
        public async Task<ActionResult> CreateByLeaveType(int leaveType) {
            var leaveTypeObj = await _LeaveTypeRepository.FindByIdAsync(leaveType);
            LeaveAllocationEditionViewModel allocationEditionViewModel = await InitializeLeaveAllocationEditionViewModel(leaveType: leaveTypeObj, includeEmployeesDictionary: false,
                includeLeaveTypesDictionary: false);
            ViewBag.Action = nameof(CreateNewByLeaveType);
            return View(CreateLeaveAllocationView, allocationEditionViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNewByLeaveType(LeaveAllocationEditionViewModel leave) {
            if (!ModelState.IsValid)
                return View(leave);
            int period = leave.Period;
            var leaveTypeObj = await _LeaveTypeRepository.FindByIdAsync(leave.AllocationLeaveTypeId);
            var identityUser = await _EmployeeRepository.FindByIdAsync(leave.AllocationEmployeeId);
            var durationValidation = await ValidateDurationOfLeaveAsync(leaveTypeObj, period, (int)leave.NumberOfDays, identityUser.Id);
            if (!durationValidation.Item1) {
                HomeController.DisplayProblem(_Logger, this, "InvalidDuration", _LocalizerFactory.MiscelanousLocalizer["Duration more that authorized by {0} days", durationValidation.Item2 * -1L]);
                leave.AllocationEmployee = _Mapper.Map<EmployeePresentationDefaultViewModel>(identityUser);
                leave.AllocationLeaveType = _Mapper.Map<LeaveTypeNavigationViewModel>(leaveTypeObj);
                return View(nameof(CreateByLeaveType), leave);
            }
            var newLeaveAllocation = new LeaveAllocation() {
                AllocationEmployeeId = leave.AllocationEmployeeId,
                AllocationLeaveTypeId = leave.AllocationLeaveTypeId,
                DateCreated = leave.DateCreated,
                NumberOfDays = leave.NumberOfDays,
                Period = period,
            };

            bool succeed = await _LeaveAllocationRepository.CreateAsync(newLeaveAllocation);
            if (succeed) {
                return await UserLeaveAllocationsForPeriod(leave.AllocationEmployeeId, leave.Period);
            }
            else
                return View(leave);
        }
        #endregion

        #region Show user allocations
        public async Task<ActionResult> UserLeaveAllocationsForPeriod(string userId = "", int? period = null, int? leaveTypeId = null) {
            if (String.IsNullOrWhiteSpace(userId)) {
                var employee = await GetCurrentEmployeeAsync();
                userId = employee.Id;
            }
            int periodVal = DateTime.Now.Year;
            IEnumerable<LeaveAllocation> leaveAllocations;
            if (period != null) {
                periodVal = (int)period;
                leaveAllocations = await _LeaveAllocationRepository.WhereAsync(
                    x => x.AllocationEmployeeId.Equals(userId)
                    && x.Period == periodVal);
            }
            else
                leaveAllocations = await _LeaveAllocationRepository.WhereAsync(
                    x => x.AllocationEmployeeId.Equals(userId));
            if (leaveTypeId != null)
                leaveAllocations = leaveAllocations.Where(x => x.AllocationLeaveTypeId.Equals(leaveTypeId));

            var allocationsLeave = _Mapper.Map<List<LeaveAllocationPresentationViewModel>>(leaveAllocations);
            return await Task.FromResult(View("UserLeaveAllocationsForPeriod", allocationsLeave));
        }
        #endregion

        // GET: LeaveAllocation
        public async Task<ActionResult> AllocateByEmployees() {
            if (!await _UserManager.IsUserHasOneRoleOfAsync(await GetCurrentEmployeeAsync(), SeedData.AdministratorRole, SeedData.HRStaffRole))
                return Forbid();
            var employeesList = _Mapper.Map<List<EmployeePresentationDefaultViewModel>>(await _EmployeeRepository.FindAllAsync());
            return View(employeesList);
        }

        public async Task<ActionResult> AllocateByEmployee(string employeeId = null) {
            Employee employee = null;
            Employee currentEmployee = await GetCurrentEmployeeAsync();
            bool isPrivelegedUser = await _UserManager.IsUserHasOneRoleOfAsync(currentEmployee, SeedData.AdministratorRole, SeedData.EmployeeRole);
            bool isCurrentUser = currentEmployee.Id.Equals(employeeId);
            bool autorized = isCurrentUser;
            if (!autorized)
                autorized |= isPrivelegedUser;
            if (!autorized)
                return Forbid();
            if (!String.IsNullOrWhiteSpace(employeeId)) {
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
            }
            ViewBag.Action = nameof(CreateNewByEmployee);
            var viewModel = await InitializeLeaveAllocationEditionViewModel(employee: employee, period: DateTime.Now.Year, includeEmployeesDictionary: true,
                allowEditLeaveType : true,
                allowEditPeriod: isPrivelegedUser,
                allowEditEmployee: isPrivelegedUser);
            return View(CreateLeaveAllocationView, viewModel);
        }

        public async Task<ActionResult> CreateNewByEmployee(LeaveAllocationEditionViewModel leaveAllocationViewModel) {
            if (!ModelState.IsValid) {
                HomeController.DisplayProblem(_Logger, this, _LocalizerFactory.MiscelanousLocalizer["Cant' save leave allocation"],
                    _LocalizerFactory.MiscelanousLocalizer["Cant' save leave allocation"]);
                ViewBag.Action = "CreateNewByEmployee";
                return View(CreateLeaveAllocationView, leaveAllocationViewModel);
            }
            #region Verification of access
            //User can edit allocatgion only for himself, and cant make the leave more that allowed
            var currentEmployee = await GetCurrentEmployeeAsync();
            bool isPrevilegedUser = await _UserManager.IsUserHasOneRoleOfAsync(currentEmployee, SeedData.AdministratorRole, SeedData.HRStaffRole);
            if (!leaveAllocationViewModel.AllocationEmployeeId.Equals(currentEmployee.Id) && !isPrevilegedUser) //Cant edit not own leave allocation
                return Forbid();
            int period = leaveAllocationViewModel.Period;
            var leaveTypeObj = await _LeaveTypeRepository.FindByIdAsync(leaveAllocationViewModel.AllocationLeaveTypeId);
            var identityUser = await _EmployeeRepository.FindByIdAsync(leaveAllocationViewModel.AllocationEmployeeId);
            var durationValidation = await ValidateDurationOfLeaveAsync(leaveTypeObj, period, (int)leaveAllocationViewModel.NumberOfDays, identityUser.Id);
            if (!durationValidation.Item1 || !isPrevilegedUser) {
                HomeController.DisplayProblem(_Logger, this, "InvalidDuration", _LocalizerFactory.MiscelanousLocalizer["Duration more that authorized by {0} days", durationValidation.Item2 * -1L]);
                leaveAllocationViewModel.AllocationEmployee = _Mapper.Map<EmployeePresentationDefaultViewModel>(identityUser);
                leaveAllocationViewModel.AllocationLeaveType = _Mapper.Map<LeaveTypeNavigationViewModel>(leaveTypeObj);
                return View("CreateLeaveAllocation", leaveAllocationViewModel);
            }
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
                return await UserLeaveAllocationsForPeriod(leaveAllocationViewModel.AllocationEmployeeId, leaveAllocationViewModel.Period);
            }
            else
                return View(leaveAllocationViewModel);
        }

        // GET: LeaveAllocation/Details/5
        public async Task<ActionResult> Details(int id) {
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if (leaveAllocation == null)
                    return NotFound();
                var currentEmployee = await GetCurrentEmployeeAsync();
                if (!leaveAllocation.AllocationEmployeeId.Equals(currentEmployee.Id)) {
                    bool isPrevilegedUser = await _UserManager.IsUserHasOneRoleOfAsync(currentEmployee, SeedData.AdministratorRole, SeedData.EmployeeRole);
                    if (!isPrevilegedUser)
                        return Forbid();
                }
                var leaveAllocationView = _Mapper.Map<LeaveAllocationPresentationViewModel>(leaveAllocation);
                return View(leaveAllocationView);
            }
            catch(AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _LocalizerFactory.MiscelanousLocalizer["Problem while getting leave allocation"], messages.ToString());
                return Redirect(Request.Headers["Referer"].ToString());
            }
            finally {
                ;
            }
            
        }


        // GET: LeaveAllocation/Edit/5
        public async Task<ActionResult> Edit(int id) {
            LeaveAllocation leaveAllocation = null;
            try {
                leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if (leaveAllocation == null)
                    return NotFound();
                var currentEmployee = await GetCurrentEmployeeAsync();
                bool isPrevilegedUser = await _UserManager.IsUserHasOneRoleOfAsync(currentEmployee, SeedData.AdministratorRole, SeedData.EmployeeRole);
                bool isConsernedEmployee = leaveAllocation.AllocationEmployeeId.Equals(currentEmployee.Id);
                if (!isConsernedEmployee) {
                    if (!isPrevilegedUser)
                        return Forbid();
                }
                var leaveAllocationView = _Mapper.Map<LeaveAllocationEditionViewModel>(leaveAllocation);
                if (isPrevilegedUser)
                    leaveAllocationView.Employees = (await _EmployeeRepository.FindAllAsync()).Select(
                        x=>new SelectListItem(x.FormatEmployeeSNT(), x.Id, x.Id.Equals(leaveAllocation.AllocationEmployeeId)));
                if(isPrevilegedUser || isConsernedEmployee)
                    leaveAllocationView.AllocationLeaveTypes = (await _LeaveTypeRepository.FindAllAsync()).Select(
                        x => new SelectListItem(x.LeaveTypeName, x.Id.ToString(), x.Id.Equals(leaveAllocation.AllocationLeaveTypeId)));
                SetEditViewProperties(allowEditEmployee: isPrevilegedUser, allowEditLeaveType: (isPrevilegedUser || isConsernedEmployee)
                    , allowEditPeriod: isPrevilegedUser);
                ViewBag.Action = nameof(Edit);
                return View(CreateLeaveAllocationView, leaveAllocationView);
            }
            catch (AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _LocalizerFactory.MiscelanousLocalizer["Problem while getting leave allocation"], messages.ToString());
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
            LeaveAllocation originalLeaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
            try {
                if (originalLeaveAllocation == null)
                    return NotFound();
                var currentEmployee = await GetCurrentEmployeeAsync();
                bool isPrevilegedUser = await _UserManager.IsUserHasOneRoleOfAsync(currentEmployee, SeedData.AdministratorRole, SeedData.EmployeeRole);
                bool isConsernedEmployee = originalLeaveAllocation.AllocationEmployeeId.Equals(currentEmployee.Id);
                if (!isConsernedEmployee) {
                    if (!isPrevilegedUser)
                        return Forbid();
                }
                #region Validation operation
                bool canChangeLeaveType = 
                    editionViewModel.AllocationLeaveTypeId.Equals(originalLeaveAllocation.AllocationLeaveTypeId) //Only concerned or privileged users can change leave type
                    || (isConsernedEmployee || isPrevilegedUser);
                bool canChangeEmployee = editionViewModel.AllocationEmployeeId.Equals(originalLeaveAllocation.AllocationEmployeeId)
                    || isPrevilegedUser; //Only privileged user can change conserned employee
                bool canChangePeriod = editionViewModel.Period.Equals(originalLeaveAllocation.Period)
                    || (isPrevilegedUser || isConsernedEmployee); //Only priveleged or cunserned users can change period
                var newLeaveType = await _LeaveTypeRepository.FindByIdAsync(editionViewModel.AllocationLeaveTypeId);
                var durationResult = await ValidateDurationOfLeaveAsync(newLeaveType, editionViewModel.Period, (int)editionViewModel.NumberOfDays, editionViewModel.AllocationEmployeeId);
                bool canChangeDuration = durationResult.Item1 || isPrevilegedUser;
                if (!canChangeLeaveType) ModelState.AddModelError("LeaveLype", _LocalizerFactory.MiscelanousLocalizer["You not allowed to change leave type"]);
                if(!canChangeEmployee) ModelState.AddModelError("LeaveEmployee", _LocalizerFactory.MiscelanousLocalizer["You not allowed to change employee"]);
                if (!canChangePeriod) ModelState.AddModelError("LeavePeriod", _LocalizerFactory.MiscelanousLocalizer["You not allowed to change period"]);
                if (!canChangePeriod) ModelState.AddModelError("LeavePeriod", _LocalizerFactory.MiscelanousLocalizer["You not allowed to change period"]);
                #endregion
                LeaveAllocation newLeaveAllocation = _Mapper.Map<LeaveAllocation>(editionViewModel);
                await WriteHistory(newLeaveAllocation, originalLeaveAllocation);
                originalLeaveAllocation.AllocationEmployeeId = newLeaveAllocation.AllocationEmployeeId;
                originalLeaveAllocation.AllocationLeaveTypeId = newLeaveAllocation.AllocationLeaveTypeId;
                originalLeaveAllocation.Period = newLeaveAllocation.Period;
                originalLeaveAllocation.NumberOfDays = newLeaveAllocation.NumberOfDays;

                bool succeed = await _LeaveAllocationRepository.UpdateAsync(originalLeaveAllocation);
                if (succeed) {
                    return await UserLeaveAllocationsForPeriod(editionViewModel.AllocationEmployeeId, editionViewModel.Period);
                }
                else {
                    ViewBag.Action = nameof(Edit);
                    return View(CreateLeaveAllocationView, editionViewModel);
                }

            }
            catch (AggregateException e) {
                var messages = new StringBuilder();
                e.Flatten().InnerExceptions.Select(x => { messages.Append(x.Message); return 0; });
                HomeController.DisplayProblem(_Logger, this, _LocalizerFactory.MiscelanousLocalizer["Problem while getting leave allocation"], messages.ToString());
                return Redirect(Request.Headers["Referer"].ToString());
            }
            finally {
                ;
            }
        }

        // GET: LeaveAllocation/Delete/5
        public async Task<ActionResult> Delete(int id) {
            object redirectData = null;
            ActionResult result;
            try {
                var leaveAllocation = await _LeaveAllocationRepository.FindByIdAsync(id);
                if(leaveAllocation != null) {
                    var data = new { userId = leaveAllocation.AllocationEmployeeId, period = leaveAllocation.Period };
                    var saveResult = await _LeaveAllocationRepository.DeleteAsync(leaveAllocation);
                    if (saveResult)
                        redirectData = data;
                }
            }
            catch(Exception e) {
                HomeController.DisplayProblem(logger: _Logger, this, e.GetType().Name, e.Message, e);
            }
            finally {
                if(redirectData != null)
                    result = RedirectToAction(nameof(UserLeaveAllocationsForPeriod), redirectData);
                else
                    result = RedirectToAction(nameof(UserLeaveAllocationsForPeriod));
            }
            return result;
        }

        #region Service operations

        /// <summary>
        /// Validates leave request about its duration
        /// </summary>
        /// <param name="currentLeaveType"></param>
        /// <param name="period">Period of leave</param>
        /// <param name="duration">Requested duration of leave</param>
        /// <param name="employeeId">Employee who requests to leave</param>
        /// <returns>
        /// Tuple indicates validity and rest of days to take if employee allocates this leave. 
        /// Negative rest means rhat employee asks more that he have 
        /// </returns>
        private async Task<Tuple<bool, long>> ValidateDurationOfLeaveAsync(LeaveType currentLeaveType, int period, int duration, string employeeId) {
            if (currentLeaveType.DefaultDays >= 0) {

                var leavesOfThisTypeData = await _LeaveAllocationRepository.WhereAsync(x => x.Period == period &&
                    x.AllocationLeaveTypeId == currentLeaveType.Id &&
                    x.AllocationEmployeeId.Equals(employeeId)
                );
                var employee = await _EmployeeRepository.FindByIdAsync(employeeId);
                var totalSimilarLeave = leavesOfThisTypeData.Sum(x => x.NumberOfDays);
                //Each five years of ancienity employee has supplementat actions
                int fiveYearsPeriods = (int)Math.Floor(DateTime.Now.Subtract(employee.EmploymentDate).TotalDays / (365 * 5));
                long rest = (currentLeaveType.DefaultDays + fiveYearsPeriods) - (totalSimilarLeave + duration);
                return await Task.FromResult(Tuple.Create(rest >= 0, rest));
            }
            else
                return await Task.FromResult(Tuple.Create(true, -1L));
        }

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
        [Authorize]
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
                        var autorizedDuration = ValidateDurationOfLeaveAsync(leaveType, effectivePeriod, 0, employee.Id).Result;
                        duration = (int)autorizedDuration.Item2;
                        leaveAllocationVM.NumberOfDays = duration >= 0 ? (uint)duration : 0;
                    }
                    leaveAllocationVM.Period = effectivePeriod;
                }
                else {
                    leaveAllocationVM = _Mapper.Map<LeaveAllocationEditionViewModel>(_LeaveAllocationRepository.FindByIdAsync((long)id).Result);
                }

                includeEmployeesDictionary &=  _UserManager.IsUserHasOneRoleOfAsync(GetCurrentEmployeeAsync().Result, SeedData.AdministratorRole, SeedData.HRStaffRole).Result;
                if (includeEmployeesDictionary) {
                    leaveAllocationVM.Employees = _EmployeeRepository.FindAllAsync()
                        .Result
                        .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.FormatEmployeeSNT(), x.Id, leaveAllocationVM.AllocationEmployeeId?.Equals(x.Id) ?? false));
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
            return await Task.FromResult(_CurrentEmployee);
        }

        public async Task<IList<string>> GetCurrentEmployeesRoles() {
            return await _UserManager.GetRolesAsync(await GetCurrentEmployeeAsync());
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
                _Logger.LogInformation(_LocalizerFactory.MiscelanousLocalizer["Edition of leaving id"],
                    compareResult.DifferencesString);
            });
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.Repository.Entity;
using LeaveManagement.ViewModels.Employee;
using LeaveManagement.ViewModels.LeaveRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace LeaveManagement.Controllers {
    [Authorize]
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class LeaveRequestsController : Controller {

        private const string StatisticsView = "ModeratorIndex";
        private const string ReviewViewName = "Review";

        #region Dependencies
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly IMapper _Mapper;
        private readonly ILeaveManagementCustomLocalizerFactory _LocalizerFactory;
        private readonly IStringLocalizer _ControllerLocalizer;
        private readonly UserManager<IdentityUser> _UserManager;

        public LeaveRequestsController(
            ILeaveManagementUnitOfWork unitOfWork,
            IMapper mapper,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            UserManager<IdentityUser> userManager,
            ILeaveTypeRepositoryAsync leaveTypeRepository,
            ILeaveAllocationRepositoryAsync leaveAllocationRepository,
            IEmployeeRepositoryAsync employeeRepository) {
            _UnitOfWork = unitOfWork;
            _Mapper = mapper;
            _LocalizerFactory = localizerFactory;
            _UserManager = userManager;
            _ControllerLocalizer = _LocalizerFactory.Create(this.GetType());
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> Index() {
            var currentUser = _UserManager.GetUserAsync(User);
            var leaveRequests = await _UnitOfWork.LeaveRequest.WhereAsync(x => x.RequestingEmployeeId.Equals(currentUser.Id));
            var leaveRequestsModel = _Mapper.Map<List<LeaveRequestDefaultViewModel>>(leaveRequests);
            LeaveRequestsStatisticsViewModel leaveRequestsStatistics = new LeaveRequestsStatisticsViewModel() {
                Moderation = false,
                LeaveRequests = leaveRequestsModel,
                TotalRequests = leaveRequests.Count,
                AcceptedRequests = leaveRequests.Count(x => x.Approuved == true),
                RejectedRequests = leaveRequests.Count(x => x.Approuved == false),
                PendingRequests = leaveRequests.Count(x => x.Approuved is null)
            };
            return View(leaveRequestsStatistics);
        }

        [HttpGet]
        public async Task<IActionResult> ModeratorIndex() {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if(currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            if (!await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _ControllerLocalizer["Your role not allows you to administrate leave requests"]);
                return Forbid();
            }
   
            var leaveRequests = await _UnitOfWork.LeaveRequest.WhereAsync(lr=>lr.LeaveType.CompanyId == currentEmployee.CompanyId &&
            lr.RequestingEmployee != null && lr.RequestingEmployee.CompanyId == currentEmployee.CompanyId);
            var leaveRequestsModel = _Mapper.Map<List<LeaveRequestDefaultViewModel>>(leaveRequests);
            ViewBag.DisplayReviewButton = true;
            LeaveRequestsStatisticsViewModel leaveRequestsStatistics = new LeaveRequestsStatisticsViewModel() {
                Moderation = true,
                LeaveRequests = leaveRequestsModel,
                TotalRequests = leaveRequests.Count,
                AcceptedRequests = leaveRequests.Count(x => x.Approuved == true),
                RejectedRequests = leaveRequests.Count(x => x.Approuved == false),
                PendingRequests = leaveRequests.Count(x => x.Approuved is null)
            };
            return View(leaveRequestsStatistics);
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            var createLeaveRequestViewModel = new CreateLeaveRequestVM();
            var leaveTypes = await _UnitOfWork.LeaveTypes.WhereAsync(x=>x.CompanyId == currentEmployee.CompanyId);
            createLeaveRequestViewModel.LeaveTypes = _Mapper.Map<List<SelectListItem>>(leaveTypes);
            createLeaveRequestViewModel.StartDate = DateTime.Now.Date;
            createLeaveRequestViewModel.EndDate = DateTime.Now.AddDays(7).Date;
            return View(createLeaveRequestViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLeaveRequestVM leaveRequest) {
            
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            if (!ModelState.IsValid) {
                return View(leaveRequest);
            }
            leaveRequest.LeaveTypes = _Mapper.Map<IEnumerable<SelectListItem>>(await _UnitOfWork.LeaveTypes.WhereAsync(c=>c.CompanyId == currentEmployee.CompanyId));

            if (DateTime.Compare(leaveRequest.StartDate, leaveRequest.EndDate) > 1) {
                ModelState.AddModelError("Date", _ControllerLocalizer["Start date must be earlier that the end date"]);
                return View(leaveRequest);
            }


            if (!await ValidateRequestDaysAsync(currentEmployee.Id, (int)leaveRequest.LeaveTypeId, leaveRequest.StartDate, leaveRequest.EndDate)) {
                ModelState.AddModelError("", _ControllerLocalizer["You have requested more days that you owned"]);
                return View(leaveRequest);
            }
            var request = new LeaveRequest() {
                ActionedDateTime = null,
                Approuved = null,
                ApprouvedBy = null,
                ApprouvedById = null,
                LeaveTypeId = (int)leaveRequest.LeaveTypeId,
                RequestedDate = DateTime.Now,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                RequestingEmployeeId = currentEmployee.Id,
                RequestComment = leaveRequest.RequestComment,
            };
            bool result = await _UnitOfWork.LeaveRequest.CreateAsync(request);
            result &= await _UnitOfWork.Save();
            if (!result) {
                ModelState.AddModelError("", _ControllerLocalizer[
                    @"Something went wrong when submitting your request. Please wait a moment and retry or contact your system administrator"]);
                return View(leaveRequest);
            }
            else
                return RedirectToAction(nameof(EmployeeRequests));
        }


        [HttpGet]
        public async Task<IActionResult> Review(int requestId) {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            if (!await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _ControllerLocalizer["Your role not allows you to administrate leave requests"]);
                return Forbid();
            }
            var leaveRequest = await _UnitOfWork.LeaveRequest.FindAsync(x=>x.Id == requestId);
            if (leaveRequest == null)
                return NotFound(_ControllerLocalizer["Leave request #{0} was not found", requestId]);
            var currentUser = await _UserManager.GetUserAsync(User);
            if (!await _UserManager.IsCompanyPrivelegedUser(currentUser))
                return Forbid(_ControllerLocalizer["Your role not allows you to administrate leave requests"]);
            var viewModel = _Mapper.Map<LeaveRequestDefaultViewModel>(leaveRequest);
            viewModel.RequestingEmployee = _Mapper.Map<EmployeePresentationDefaultViewModel>(leaveRequest.RequestingEmployee);
            viewModel.ApprouvedBy = _Mapper.Map<EmployeePresentationDefaultViewModel>(leaveRequest.ApprouvedBy);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Review(int requestId, LeaveRequestDefaultViewModel viewModel) {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            if (!await _UserManager.IsCompanyPrivelegedUser(currentEmployee)) {
                ModelState.AddModelError("", _ControllerLocalizer["You not authorized to review leave requests"]);
                Forbid();
            }
            var leaveRequest = await _UnitOfWork.LeaveRequest.FindAsync(x=>x.Id == requestId);
            if (leaveRequest == null) {
                return NotFound(_ControllerLocalizer["Leave request #{0} was not found", requestId]);
            }
            if(leaveRequest.RequestingEmployee?.CompanyId != currentEmployee.CompanyId) {
                ModelState.AddModelError("", _ControllerLocalizer["You attempt to review leave request which belongs to other company "]);
                Forbid();
            }
                
            var approuve = viewModel.Approuved;
            if (approuve == true && !await ValidateRequestDaysAsync(leaveRequest.RequestingEmployeeId,
                leaveRequest.LeaveTypeId,
                leaveRequest.StartDate,
                leaveRequest.EndDate)) {
                /*HomeController.DisplayProblem(null, this, _ControllerLocalizer["Request exeeds number of days"],
                    _ControllerLocalizer["Impossible to allocate the leave "]);*/
                ModelState.AddModelError("", _ControllerLocalizer["Employee requested more days that was allocated to him"]);
                return View(viewModel);
            }
            leaveRequest.Approuved = approuve;
            leaveRequest.ActionedDateTime = DateTime.Now;
            leaveRequest.ApprouvedById = currentEmployee.Id;
            leaveRequest.RequestCancelled = viewModel.RequestCancelled;
            leaveRequest.ValidationComment = viewModel.ValidationComment;
            bool result = await _UnitOfWork.LeaveRequest.UpdateAsync(leaveRequest);
            result &= await _UnitOfWork.Save();
            if (!result) {
                ModelState.AddModelError("", _ControllerLocalizer["Failed to action the request"]);
                return View(viewModel);
            }
            else {
                return RedirectToAction(nameof(ModeratorIndex));
            }

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EmployeeRequests(int? period) {
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            int currentPeriod = period == null ? DateTime.Now.Year : (int)period;

            var requests = (await _UnitOfWork.LeaveRequest.WhereAsync(filter: q => q.RequestingEmployeeId.Equals(currentEmployee.Id),
                order: o => o.OrderByDescending(el => el.RequestedDate).ThenBy(el => el.StartDate),
                includes: new System.Linq.Expressions.Expression<Func<LeaveRequest, object>>[] { x => x.RequestingEmployee,
                    x => x.LeaveType, x=>x.ApprouvedBy })).ToList();
            
            Task<List<LeaveRequestDefaultViewModel>> mappingRequestsTask = Task.Run(() => _Mapper.Map<List<LeaveRequestDefaultViewModel>>(requests));
            Task<IDictionary<int, LeaveSold>> requestedLeaveSoldsClass = Task.Run(() => {
                var requestsGroups = requests.GroupBy(x => x.LeaveTypeId);
                IDictionary<int, LeaveSold> leaveSolds = new Dictionary<int, LeaveSold>();
                foreach(var group in requestsGroups) {
                    LeaveSold result = new LeaveSold() {
                        LeaveTypeId = group.Key,
                        UsedDays = (int)group.Where(q => q.StartDate.CompareTo(DateTime.Now) <= 0 && q.Approuved == true && !q.RequestCancelled).Sum(s => (s.EndDate - s.StartDate).TotalDays),
                        PendingDays = (int)group.Where(q => q.Approuved == null && !q.RequestCancelled).Sum(s => (s.EndDate - s.StartDate).TotalDays),
                        ApprouvedDays = (int)group.Where(q => q.Approuved == true && !q.RequestCancelled).Sum(s => (s.EndDate - s.StartDate).TotalDays),
                        ApprouvedNotUsed = (int)group.Where(q => q.StartDate.CompareTo(DateTime.Now) > 0 && q.Approuved == true && !q.RequestCancelled).Sum(s => (s.EndDate - s.StartDate).TotalDays),
                        RejectedDays = (int)group.Where(q => q.Approuved == false).Sum(s => (s.EndDate - s.StartDate).TotalDays),
                    };
                    leaveSolds.Add(result.LeaveTypeId, result);
                }
                return leaveSolds;
                });

            var allocations = await _UnitOfWork.LeaveAllocations.WhereAsync(filter: q => q.AllocationEmployeeId.Equals(currentEmployee.Id),
                includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationLeaveType, x => x.AllocationEmployee }) ;
            
            Task<IDictionary<int, LeaveSold>> allocatedLeaveSoldsTask = Task.Run(() => {
                var grouppedAllocations = allocations.GroupBy(g => g.AllocationLeaveTypeId);
                IDictionary<int, LeaveSold> result = new Dictionary<int, LeaveSold>();
                foreach(var group in grouppedAllocations) {
                    LeaveSold sold = new LeaveSold() {
                        LeaveTypeId = group.Key,
                        AllocatedDays = group.Sum(s => s.NumberOfDays)
                    };
                    result.Add(group.Key, sold);
                }
                return result;
            });

            var leaveTypes = (await _UnitOfWork.LeaveTypes.WhereAsync(lt=>lt.CompanyId == currentEmployee.CompanyId)).Select(v => new LeaveSold() {
                LeaveTypeId = v.Id,
                LeaveTypeName = v.LeaveTypeName,
                DefaultDays = v.DefaultDays
            });
            var mergedData = Task.WhenAll(requestedLeaveSoldsClass, allocatedLeaveSoldsTask).ContinueWith<IList<LeaveSold>>((req) => {
                var requestsData = requestedLeaveSoldsClass.Result;
                var allocationsData = allocatedLeaveSoldsTask.Result;
                List<LeaveSold> records = new List<LeaveSold>();
                foreach(var data in leaveTypes) {
                    if (requestsData.ContainsKey(data.LeaveTypeId)) {
                        var rd = requestsData[data.LeaveTypeId];
                        data.PendingDays = rd.PendingDays;
                        data.RejectedDays = rd.RejectedDays;
                        data.UsedDays = rd.UsedDays;
                        data.ApprouvedDays = rd.ApprouvedDays;
                        data.ApprouvedNotUsed = rd.ApprouvedNotUsed;

                    }
                    if (allocationsData.ContainsKey(data.LeaveTypeId)) {
                        var ad = allocationsData[data.LeaveTypeId];
                        data.AllocatedDays = ad.AllocatedDays;
                    }
                    data.RestOfDays = data.AllocatedDays - data.UsedDays;
                    records.Add(data);
                }
                return records;
            });
            Task.WaitAll(mappingRequestsTask, mergedData);
            EmployeeLeaveRequestsViewModel viewModel = new EmployeeLeaveRequestsViewModel() {
                LeaveRequests = mappingRequestsTask.Result,
                LeaveAllocations = mergedData.Result
            };
            return View("EmployeeLeaveRequests", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> RemoveRequest(long requestId) {
            var request = await _UnitOfWork.LeaveRequest.FindAsync(x=>x.Id == requestId);
            var currentEmployee = await _UnitOfWork.Employees.GetEmployeeAsync(User);
            if (currentEmployee == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Your account not belongs to employees"]);
                return Forbid();
            }
            if (request == null) {
                ModelState.AddModelError("", _ControllerLocalizer["Leave request not found"]);
                return NotFound();
            }

            bool userAutorizedRemoveRequest = await _UserManager.IsCompanyPrivelegedUser(User) || request.RequestingEmployeeId.Equals(currentEmployee.Id);
            userAutorizedRemoveRequest &= request.RequestingEmployee?.CompanyId == currentEmployee.CompanyId;
            bool statePermits = request.Approuved != false && !request.RequestCancelled;
            bool datePermits = request.StartDate.CompareTo(DateTime.Now.AddDays(3)) >= 1;
            if (!userAutorizedRemoveRequest) {
                ModelState.AddModelError("", _ControllerLocalizer["You cant cancel this request"]);
                return Unauthorized();
            }
            if (!statePermits || !datePermits) {
                if(!statePermits)
                    ModelState.AddModelError("", _ControllerLocalizer["Unable cancel request. Request was already cancelled or rejected.", requestId]);
                if(!datePermits)
                    ModelState.AddModelError("", _ControllerLocalizer["Unable cancel request. In starts in less then in 3 days", requestId]);
                return BadRequest(ModelState);
            }
                
            request.RequestCancelled = true;
            bool result = await _UnitOfWork.LeaveRequest.UpdateAsync(request);
            result &= await _UnitOfWork.Save();
            if (!result) {
                ModelState.AddModelError("", _ControllerLocalizer["Database error while updating leave request #{0}", requestId]);
                return BadRequest(ModelState);
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        #region Validation methods
        private async Task<bool> ValidateRequestDaysAsync(string employeeId, int leaveTypeId, DateTime startDate, DateTime endDate) {
            var allocationsForThisEmployee = await _UnitOfWork.LeaveAllocations.WhereAsync(filter: x =>
                x.AllocationLeaveTypeId == leaveTypeId
                && x.AllocationEmployeeId.Equals(employeeId),
                includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationLeaveType, x => x.AllocationEmployee }
            ) ;

            var requestsForThisEmployee = await _UnitOfWork.LeaveRequest.WhereAsync(filter: q => q.LeaveTypeId == leaveTypeId
                && q.RequestingEmployeeId.Equals(employeeId)
            ) ;

            int requestedDays = (int)(endDate - startDate).TotalDays;
            int totalAllocatedForThisEmployee = allocationsForThisEmployee.Sum(a => a.NumberOfDays);
            int totalValidatedForThisEmployee = (int)requestsForThisEmployee
                .Where(q => q.Approuved == true)
                .Sum(x => ((x.EndDate - x.StartDate).TotalDays));
            int totalPending = (int)requestsForThisEmployee
                .Where(q => q.Approuved == null)
                .Sum(x => ((x.EndDate - x.StartDate).TotalDays));

            return totalAllocatedForThisEmployee >= (totalValidatedForThisEmployee + requestedDays);
        }

        private static bool ValidateDays(DateTime startDate, DateTime endDate, int[] daysInPeriods) {
            int periods = endDate.Year - startDate.Year;
            if (daysInPeriods.Length != periods + 1)
                throw new ArgumentException("Length of daysInPeriods must be equal to number of periods between startDate and endDate");
            bool periodsValids = true;
            int startYear = startDate.Year;
            DateTime periodStarts = startDate;
            DateTime periodEnds = startDate.Year == endDate.Year ? endDate : new DateTime(startYear, 12, 31);
            for (int year = startYear; year <= endDate.Year; year++) {
                periodsValids &= (periodEnds - periodStarts).TotalDays <= daysInPeriods[year - startYear];
                periodStarts = new DateTime(year + 1, 1, 1);
                periodEnds = DateTime.Compare(endDate, new DateTime(year + 1, 12, 31)) <= 0 ? endDate :
                     new DateTime(year + 1, 12, 31);
            }
            return periodsValids;
        }
        #endregion
    }
}
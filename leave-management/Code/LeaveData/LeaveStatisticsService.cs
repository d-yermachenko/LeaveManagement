using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.LeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LeaveManagement.Code.LeaveData {
    public class LeaveStatisticsService : ILeaveStatisticsService {
        ILeaveManagementUnitOfWork _UnitOfWork;

        public LeaveStatisticsService(ILeaveManagementUnitOfWork unitOfWork
            ) {
            _UnitOfWork = unitOfWork;
        }



        public async Task<IList<LeaveSold>> GetLeaveStatistics(IEnumerable<LeaveRequest> requests, Employee consernedEmployee) {
            Task<IDictionary<int, LeaveSold>> requestedLeaveSoldsClass = Task.Run(() => {
                var requestsGroups = requests.GroupBy(x => x.LeaveTypeId);
                IDictionary<int, LeaveSold> leaveSolds = new Dictionary<int, LeaveSold>();
                foreach (var group in requestsGroups) {
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

            var allocations = await _UnitOfWork.LeaveAllocations.WhereAsync(filter: q => q.AllocationEmployeeId.Equals(consernedEmployee.Id),
                includes: new System.Linq.Expressions.Expression<Func<LeaveAllocation, object>>[] { x => x.AllocationLeaveType, x => x.AllocationEmployee });

            Task<IDictionary<int, LeaveSold>> allocatedLeaveSoldsTask = Task.Run(() => {
                var grouppedAllocations = allocations.GroupBy(g => g.AllocationLeaveTypeId);
                IDictionary<int, LeaveSold> result = new Dictionary<int, LeaveSold>();
                foreach (var group in grouppedAllocations) {
                    LeaveSold sold = new LeaveSold() {
                        LeaveTypeId = group.Key,
                        AllocatedDays = group.Sum(s => s.NumberOfDays)
                    };
                    result.Add(group.Key, sold);
                }
                return result;
            });

            var leaveTypes = (await _UnitOfWork.LeaveTypes.WhereAsync(lt => lt.CompanyId == consernedEmployee.CompanyId)).Select(v => new LeaveSold() {
                LeaveTypeId = v.Id,
                LeaveTypeName = v.LeaveTypeName,
                DefaultDays = v.DefaultDays
            });
            var mergedData = Task.WhenAll(requestedLeaveSoldsClass, allocatedLeaveSoldsTask).ContinueWith<IList<LeaveSold>>((req) => {
                var requestsData = requestedLeaveSoldsClass.Result;
                var allocationsData = allocatedLeaveSoldsTask.Result;
                List<LeaveSold> records = new List<LeaveSold>();
                foreach (var data in leaveTypes) {
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

            return await mergedData;
        }

        public async Task<IList<LeaveSold>> GetLeaveStatistics(Employee consernedEmployee) {
            if (consernedEmployee == null)
                return new List<LeaveSold>() { };
            var requests = await _UnitOfWork.LeaveRequest.WhereAsync(
               filter: x => x.RequestingEmployeeId == consernedEmployee.Id,
               includes: new System.Linq.Expressions.Expression<Func<LeaveRequest, object>>[] { x => x.RequestingEmployee, x => x.LeaveType, x => x.ApprouvedBy });
            return await GetLeaveStatistics(requests, consernedEmployee);
        }

        public async Task<IList<LeaveSold>> GetLeaveStatistics(ClaimsPrincipal user) {
            if (!user.Identity.IsAuthenticated)
                return new List<LeaveSold>() { };
            var consernedEmployee = await _UnitOfWork.Employees.FindAsync(predicate: x => x.UserName == user.Identity.Name);
            return await GetLeaveStatistics(consernedEmployee);


        }

        #region Disposing
        private bool _Disposed;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if (_Disposed)
                return;
            if (disposing) {
                _UnitOfWork.Dispose();
            }
            _Disposed = true;
        }
        #endregion
    }
}

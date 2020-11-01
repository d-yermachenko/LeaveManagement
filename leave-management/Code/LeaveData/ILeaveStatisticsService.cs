using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.LeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code.LeaveData {
    public interface ILeaveStatisticsService: IDisposable {
        Task<IList<LeaveSold>> GetLeaveStatistics(IEnumerable<LeaveRequest> requests, Employee consernedEmployee);

        Task<IList<LeaveSold>> GetLeaveStatistics(System.Security.Claims.ClaimsPrincipal user);

        Task<IList<LeaveSold>> GetLeaveStatistics(Employee consernedEmployee);
    }
}

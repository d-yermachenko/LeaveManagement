using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Contracts {
    public interface ILeaveAllocationRepositoryAsync : IRepositoryBaseAsync<LeaveAllocation, long>{
        Task<IEnumerable<LeaveAllocation>> WhereAsync(Func<LeaveAllocation, bool> predicate);
    }
}

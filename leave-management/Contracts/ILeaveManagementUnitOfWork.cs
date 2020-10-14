using LeaveManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts {
    public interface ILeaveManagementUnitOfWork {
        public IRepository<LeaveType> LeaveTypes { get; }

        public IRepository<LeaveAllocation> LeaveAllocations { get; }

        public IRepository<LeaveRequest> LeaveRequest { get; }

        public IRepository<Employee> Employees { get; }

        public IRepository<Company> Companies { get; }

        public Task<bool> Save();
    }
}

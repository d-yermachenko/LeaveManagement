using LeaveManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts {
    public interface IApplicationDbContext : ISavable {
        IDatabaseSet<Employee> EmployeesData { get; }

        IDatabaseSet<LeaveHistory> LeaveHistoriesData { get; }

        IDatabaseSet<LeaveType> LeaveTypesData { get; }

        IDatabaseSet<LeaveAllocation> LeaveAllocationsData { get; }

    }
}

using LeaveManagement.Data.Entities;
using System.Collections.Generic;

namespace LeaveManagement.Contracts {
    public interface ILeaveTypeRepository : IRepositoryBase<LeaveType, int> {
        ICollection<LeaveType> GetLeaveTypesByEmployeeId(string employeeId);

    }
}

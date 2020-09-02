using System;
using System.Collections.Generic;
using System.Linq;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Contracts {
    public interface ILeaveRequestsRepositoryAsync : IRepositoryBaseAsync<LeaveRequest, long> {
       
    }
}

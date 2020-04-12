﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Contracts {
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation, long>{
    }
}

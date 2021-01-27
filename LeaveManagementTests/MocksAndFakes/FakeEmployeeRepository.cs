using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagementTests.MocksAndFakes {
    public class FakeEmployeeRepository : MockRepository<string, Employee> {

        public FakeEmployeeRepository() : base(x => x.Id) {

        }
    }
}


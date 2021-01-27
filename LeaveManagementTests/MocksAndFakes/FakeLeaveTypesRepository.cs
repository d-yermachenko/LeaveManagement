using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LeaveManagementTests.MocksAndFakes {
    public class MockLeaveTypesRepository : MockRepository<int, LeaveType> {

        public MockLeaveTypesRepository() : base(x => x.Id) {

        }

        protected void FillLeaveTypesStorage() {
            Func<int, LeaveType, LeaveType> leaveTypeUpdateMethod = delegate (int key, LeaveType value) {
                return value;
            };
            Task.WaitAll(new Task[] {
                CreateAsync(new LeaveType() {
                Id = 1,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 15),
                LeaveTypeName = "Illness",
                DefaultDays = -1
            }),
            CreateAsync(
                new LeaveType() {
                Id = 2,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 30),
                LeaveTypeName = "Vacation",
                DefaultDays = -1
            }),
                CreateAsync(new LeaveType() {
                Id = 3,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "Pregnacy",
                DefaultDays = 30
            }),
                CreateAsync(new LeaveType() {
                Id = 4,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "Foreign mission",
                DefaultDays = -1
            }),
                CreateAsync(new LeaveType() {
                Id = 5,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "RTT",
                DefaultDays = -1
            })
            });

        }
    }
}

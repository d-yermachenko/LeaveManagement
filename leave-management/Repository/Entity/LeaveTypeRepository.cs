using System;
using System.Collections.Generic;
using System.Linq;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Repository.Entity {
    public class LeaveTypeRepository : ILeaveTypeRepository {

        ApplicationDbContext ApplicationDbContext;

        public LeaveTypeRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }

        public bool Create(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Add(entity);
                return ApplicationDbContext.Save();
            }
            catch {
                throw;
            }
        }

        public bool Delete(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Remove(entity);
                return ApplicationDbContext.Save();
            }
            catch {
                throw;
            }
        }

        public ICollection<LeaveType> FindAll() => ApplicationDbContext.LeaveTypes.ToArray();

        public LeaveType FindById(int id) {
            return ApplicationDbContext.LeaveTypes.Find( id );
        }

        public ICollection<LeaveType> GetLeaveTypesByEmployeeId(string employeeId) {
            Func<LeaveType, bool> selectExpression = (leaveType) => {
                var historyOfEmployee = ApplicationDbContext.LeaveHistories.Where(lh => lh.RequestingEmployeeId.Equals(employeeId));
                var leaveTypesIds = historyOfEmployee.Select(hoe => hoe.LeaveTypeId).GroupBy(lti => lti).Select(gr => gr.Key);
                return leaveTypesIds.Contains(leaveType.Id);
            };
            return ApplicationDbContext.LeaveTypes.Where(x=> selectExpression.Invoke(x)).ToArray();
        }

        public bool Save() {
            bool result = false;
            try {
                ApplicationDbContext.Save();
                result = true;
            }
            catch {
                throw;
            }
            return result;
        }

        public bool Update(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Update(entity);
                return Save();
            }
            catch {
                throw;
            }
        }
    }
}

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
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypesData.Add(entity);
                ApplicationDbContext.Save();
                result = true;
            }
            catch {
                throw;
            }
            return result;
        }

        public bool Delete(LeaveType entity) {
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypesData.Remove(entity);
                ApplicationDbContext.Save();
                result = true;
            }
            catch {
                throw;
            }
            return result;
        }

        public ICollection<LeaveType> FindAll() => ApplicationDbContext.LeaveTypesData.ToArray();

        public LeaveType FindById(int id) {
            return ApplicationDbContext.LeaveTypesData.Find(new object[] { id });
        }

        public ICollection<LeaveType> GetLeaveTypesByEmployeeId(string employeeId) {
            Func<LeaveType, bool> selectExpression = (leaveType) => {
                var historyOfEmployee = ApplicationDbContext.LeaveHistoriesData.Where(lh => lh.RequestingEmployeeId.Equals(employeeId));
                var leaveTypesIds = historyOfEmployee.Select(hoe => hoe.LeaveTypeId).GroupBy(lti => lti).Select(gr => gr.Key);
                return leaveTypesIds.Contains(leaveType.Id);
            };
            return ApplicationDbContext.LeaveTypesData.Where(x=> selectExpression.Invoke(x)).ToArray();
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
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypesData.Update(entity);
                result = true;
            }
            catch {
                throw;
            }
            return result;
        }
    }
}

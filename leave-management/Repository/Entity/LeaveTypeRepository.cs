using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Repository.Entity {
    public class LeaveTypeRepository : ILeaveTypeRepositoryAsync {

        ApplicationDbContext ApplicationDbContext;

        public LeaveTypeRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }

        public async Task<bool> CreateAsync(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Add(entity);
                return await SaveAsync();
            }
            catch {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Remove(entity);
                return await SaveAsync();
            }
            catch {
                throw;
            }
        }

        public async Task<ICollection<LeaveType>> FindAllAsync() => await ApplicationDbContext.LeaveTypes.ToArrayAsync();

        public async Task<LeaveType> FindByIdAsync(int id) {
            return await ApplicationDbContext.LeaveTypes.FindAsync( id );
        }

        public async Task<ICollection<LeaveType>> GetLeaveTypesByEmployeeIdAsync(string employeeId) {
            Func<LeaveType, bool> selectExpression = (leaveType) => {
                var historyOfEmployee = ApplicationDbContext.LeaveHistories.Where(lh => lh.RequestingEmployeeId.Equals(employeeId));
                var leaveTypesIds = historyOfEmployee.Select(hoe => hoe.LeaveTypeId).GroupBy(lti => lti).Select(gr => gr.Key);
                return leaveTypesIds.Contains(leaveType.Id);
            };
            return await Task.Run(() => {
                return ApplicationDbContext.LeaveTypes.Where(x => selectExpression.Invoke(x)).ToArray();
            });
        }

        public async Task<bool> SaveAsync() {
            try {
                return (await ApplicationDbContext.SaveChangesAsync()) > 0;
                
            }
            catch {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(LeaveType entity) {
            try {
                ApplicationDbContext.LeaveTypes.Update(entity);
                return await SaveAsync();
            }
            catch {
                throw;
            }
        }
    }
}

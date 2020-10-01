using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Repository.Entity {
    public class LeaveRequestsRepository : ILeaveRequestsRepositoryAsync {
        public ApplicationDbContext ApplicationDbContext;
        public ILogger<LeaveRequestsRepository> _Logger;

        public LeaveRequestsRepository(ApplicationDbContext applicationDbContext,
            ILogger<LeaveRequestsRepository> logger) {
            ApplicationDbContext = applicationDbContext;
            _Logger = logger;
        }


        public async Task<bool> CreateAsync(LeaveRequest entity) {
            ApplicationDbContext.LeaveRequests.Add(entity);
            return await SaveAsync();
        }

        public async Task<bool> DeleteAsync(LeaveRequest entity) {
            ApplicationDbContext.LeaveRequests.Remove(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<LeaveRequest>> FindAllAsync() => await ApplicationDbContext.LeaveRequests
            .Include(x => x.ApprouvedBy)
            .Include(x => x.RequestingEmployee)
            .Include(x => x.LeaveType)
            .ToArrayAsync();

        public async Task<LeaveRequest> FindByIdAsync(long id) => await ApplicationDbContext.LeaveRequests
            .Include(x => x.ApprouvedBy)
            .Include(x => x.RequestingEmployee)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<bool> SaveAsync() {
            var affectedRecords = await ApplicationDbContext.SaveChangesAsync();
            return affectedRecords > 0;
        }

        public async Task<bool> UpdateAsync(LeaveRequest entity) {
            ApplicationDbContext.LeaveRequests.Update(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<LeaveRequest>> WhereAsync(Func<LeaveRequest, bool> predicate) {
            return await Task.Run<ICollection<LeaveRequest>>(() => {
                try {
                  return ApplicationDbContext.LeaveRequests
                 .Include(x => x.ApprouvedBy)
                 .Include(x => x.RequestingEmployee)
                 .Include(x => x.LeaveType)
                 .Where(predicate).ToList();
                }
                catch (Exception e) {
                    _Logger.LogError(e, e.Message);
                    return new LeaveRequest[] { };
                }
            });
        }
    }
}

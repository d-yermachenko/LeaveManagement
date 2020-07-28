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

namespace LeaveManagement.Repository.Entity {
    public class LeaveHistoryRepository : ILeaveHistoryRepositoryAsync {
        public ApplicationDbContext ApplicationDbContext;

        public LeaveHistoryRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }


        public async Task<bool> CreateAsync(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistories.Add(entity);
            return await SaveAsync();
        }

        public async Task<bool> DeleteAsync(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistories.Remove(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<LeaveHistory>> FindAllAsync() => await ApplicationDbContext.LeaveHistories
            .Include(x => x.ApprouvedBy)
            .Include(x => x.RequestingEmployee)
            .Include(x => x.LeaveType)
            .ToArrayAsync();

        public async Task <LeaveHistory> FindByIdAsync(long id) => await ApplicationDbContext.LeaveHistories
            .Include(x=>x.ApprouvedBy)
            .Include(x=>x.RequestingEmployee)
            .Include(x=>x.LeaveType)
            .FirstOrDefaultAsync(x=>x.Id == id);

        public async Task<bool> SaveAsync() {
            var affectedRecords = await ApplicationDbContext.SaveChangesAsync();
            return affectedRecords> 0;
        }

        public async Task<bool> UpdateAsync(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistories.Update(entity);
            return await SaveAsync();
        }
    }
}

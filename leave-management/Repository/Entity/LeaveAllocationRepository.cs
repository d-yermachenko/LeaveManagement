using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Repository.Entity {
    public class LeaveAllocationRepository : ILeaveAllocationRepositoryAsync {
        private ApplicationDbContext ApplicationDbContext;

        public LeaveAllocationRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }


        public async Task<bool> CreateAsync(LeaveAllocation entity) {
                try {
                    ApplicationDbContext.LeaveAllocations.Add(entity);
                    return await SaveAsync();
                }
                catch {
                    throw;
                }
               
        }

        public async Task<bool> DeleteAsync(LeaveAllocation entity) {
            try {
                ApplicationDbContext.LeaveAllocations.Remove(entity);
                return await SaveAsync();
            }
            catch {
                throw;
            }
        }

        public async Task<ICollection<LeaveAllocation>> FindAllAsync() => 
            await ApplicationDbContext.LeaveAllocations
            .Include(ai => ai.AllocationEmployee)
            .ThenInclude(emc=>emc.Company)
            .Include(lt => lt.AllocationLeaveType)
            .ThenInclude(ltc=>ltc.Company)
            .ToListAsync();

        public async Task<LeaveAllocation> FindByIdAsync(long id) => await ApplicationDbContext.LeaveAllocations
            .Include(ai => ai.AllocationEmployee)
            .ThenInclude(emc => emc.Company)
            .Include(lt => lt.AllocationLeaveType)
            .ThenInclude(ltc => ltc.Company)
            .FirstOrDefaultAsync(x=>x.Id == id);

        public async Task<bool> SaveAsync() {
            return await Task.Run(() => {
                var affectedLines = ApplicationDbContext.SaveChanges();
                return affectedLines > 0;
            });
        }

        public async Task<bool> UpdateAsync(LeaveAllocation entity) {
            ApplicationDbContext.LeaveAllocations.Update(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<LeaveAllocation>> WhereAsync(Func<LeaveAllocation, bool> predicate) {
            return await Task.Run(()=>ApplicationDbContext.LeaveAllocations
            .Include(ae=>ae.AllocationEmployee)
            .ThenInclude(aec => aec.Company)
            .Include(lt=>lt.AllocationLeaveType)
            .ThenInclude(lte => lte.Company)
            .Where(predicate).ToList());
        }

    }
}

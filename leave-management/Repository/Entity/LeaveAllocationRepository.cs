using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Repository.Entity {
    public class LeaveAllocationRepository : ILeaveAllocationRepository {
        private ApplicationDbContext ApplicationDbContext;

        public LeaveAllocationRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }


        public bool Create(LeaveAllocation entity) {
            try {
                ApplicationDbContext.LeaveAllocations.Add(entity);
                return ApplicationDbContext.SaveChanges() > 0;
            }
            catch {
                throw;
            }

        }

        public bool Delete(LeaveAllocation entity) {
            try {
                ApplicationDbContext.LeaveAllocations.Remove(entity);
                return true;
            }
            catch {
                throw;
            }
        }

        public ICollection<LeaveAllocation> FindAll() => ApplicationDbContext.LeaveAllocations.ToList();

        public LeaveAllocation FindById(long id) => ApplicationDbContext.LeaveAllocations.Find(new long[] { id });

        public bool Save() {
            ApplicationDbContext.Save();
            return true;
        }

        public bool Update(LeaveAllocation entity) {
            ApplicationDbContext.LeaveAllocations.Update(entity);
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Repository.Generic {
    public class LeaveAllocationRepository : ILeaveAllocationRepository {
        private IApplicationDbContext ApplicationDbContext;

        public LeaveAllocationRepository(IApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }


        public bool Create(LeaveAllocation entity) {
            try {
                ApplicationDbContext.LeaveAllocationsData.Add(entity);
                return true;
            }
            catch {
                throw;
            }

        }

        public bool Delete(LeaveAllocation entity) {
            try {
                ApplicationDbContext.LeaveAllocationsData.Remove(entity);
                return true;
            }
            catch {
                throw;
            }
        }

        public ICollection<LeaveAllocation> FindAll() => ApplicationDbContext.LeaveAllocationsData.ToList();

        public LeaveAllocation FindById(long id) => ApplicationDbContext.LeaveAllocationsData.Find(new long[] { id });

        public bool Save() {
            ApplicationDbContext.Save();
            return true;
        }

        public bool Update(LeaveAllocation entity) {
            ApplicationDbContext.LeaveAllocationsData.Update(entity);
            return true;
        }
    }
}

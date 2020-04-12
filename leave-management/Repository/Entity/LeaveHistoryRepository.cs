using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;

namespace LeaveManagement.Repository.Entity {
    public class LeaveHistoryRepository : ILeaveHistoryRepository {
        public ApplicationDbContext ApplicationDbContext;

        public LeaveHistoryRepository(ApplicationDbContext applicationDbContext) {
            ApplicationDbContext = applicationDbContext;
        }


        public bool Create(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistoriesData.Add(entity);
            return true;
        }

        public bool Delete(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistoriesData.Remove(entity);
            return true;
        }

        public ICollection<LeaveHistory> FindAll() => ApplicationDbContext.LeaveHistoriesData.ToArray();

        public LeaveHistory FindById(long id) => ApplicationDbContext.LeaveHistoriesData.Find(new object[] { id });

        public bool Save() {
            ApplicationDbContext.Save();
            return true;
        }

        public bool Update(LeaveHistory entity) {
            ApplicationDbContext.LeaveHistoriesData.Update(entity);
            return true;
        }
    }
}

using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagementTests.MocksAndFakes {
    public class FakeLeaveTypesRepository : ILeaveTypeRepositoryAsync {

        private ConcurrentDictionary<int, LeaveType> _LeaveTypeStorage;

        public FakeLeaveTypesRepository() {
            _LeaveTypeStorage = new ConcurrentDictionary<int, LeaveType>();
            FillLeaveTypesStorage();
        }

        protected void FillLeaveTypesStorage() {
            Func<int, LeaveType, LeaveType>  leaveTypeUpdateMethod = delegate (int key, LeaveType value) {
                return value;
            };

            _LeaveTypeStorage.AddOrUpdate(1, new LeaveType() {
                Id = 1,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 15),
                LeaveTypeName = "Illness",
                DefaultDays = -1
            }, leaveTypeUpdateMethod);
            _LeaveTypeStorage.AddOrUpdate(2, new LeaveType() {
                Id = 2,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 30),
                LeaveTypeName = "Vacation",
                DefaultDays = -1
            }, leaveTypeUpdateMethod);
            _LeaveTypeStorage.AddOrUpdate(3, new LeaveType() {
                Id = 3,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "Pregnacy",
                DefaultDays = 30
            }, leaveTypeUpdateMethod);
            _LeaveTypeStorage.AddOrUpdate(4, new LeaveType() {
                Id = 4,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "Foreign mission",
                DefaultDays = -1
            }, leaveTypeUpdateMethod);
            _LeaveTypeStorage.AddOrUpdate(5, new LeaveType() {
                Id = 5,
                DateCreated = new DateTime(2020, 8, 1, 8, 30, 45),
                LeaveTypeName = "RTT",
                DefaultDays = -1
            }, leaveTypeUpdateMethod);
        }


        public async Task<bool> CreateAsync(LeaveType entity) {
            return await Task.Run<bool>(() => {
                if (_LeaveTypeStorage.Any(ent => ent.Key == entity.Id || ent.Value.LeaveTypeName.Equals(entity.LeaveTypeName)))
                    return false;
                _LeaveTypeStorage.AddOrUpdate(entity.Id, entity, (id, leaveType) => leaveType);
                return true;
            });
        }

        public async Task<bool> DeleteAsync(LeaveType entity) {
            return await Task.Run<bool>(() => {
                if (!_LeaveTypeStorage.Any(ent => ent.Key == entity.Id || ent.Value.LeaveTypeName.Equals(entity.LeaveTypeName)))
                    return false;
                var entityToRemove = _LeaveTypeStorage.First(ent => ent.Key == entity.Id || ent.Value.LeaveTypeName.Equals(entity.LeaveTypeName));
                return _LeaveTypeStorage.Remove(entity.Id, out LeaveType removedLeaveType);
            });
        }

        public Task<ICollection<LeaveType>> FindAllAsync() {
            throw new NotImplementedException();
        }

        public Task<LeaveType> FindByIdAsync(int id) {
            throw new NotImplementedException();
        }


        public Task<bool> SaveAsync() {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(LeaveType entity) {
            throw new NotImplementedException();
        }

        public Task<ICollection<LeaveType>> WhereAsync(Func<LeaveType, bool> predicate) {
            throw new NotImplementedException();
        }
    }
}

using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagementTests.MocksAndFakes {
    public class FakeEmployeeRepository : IEmployeeRepositoryAsync {
        public ConcurrentDictionary<string, Employee> _Employees;

        public async Task<bool> CreateAsync(Employee entity) {
            return await Task.FromResult<bool>(_Employees.TryAdd(entity.Id, entity));
        }

        public async Task<bool> DeleteAsync(Employee entity) {
            return await Task.FromResult<bool>(_Employees.Remove(entity.Id, out Employee removedEmployee));
        }

        public async Task<ICollection<Employee>> FindAllAsync() {
            return await Task.FromResult(_Employees.Values);
        }

        public async Task<Employee> FindByIdAsync(string id) {
            return await Task.FromResult(_Employees[id]);
        }

        public async Task<Employee> GetEmployeeAsync(ClaimsPrincipal user) {
            return await Task.FromResult(_Employees.Values.Where(e=>e.NormalizedUserName == user.Identity.Name).FirstOrDefault());
        }

        public async Task<bool> RegisterEmployeeAsync(Employee employee, string password) {
            return await Task.FromResult<bool>(_Employees.TryAdd(employee.Id, employee));
        }

        public async Task<bool> SaveAsync() {
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateAsync(Employee entity) {
            return await Task.Run(() => {
                return _Employees.TryUpdate(entity.Id, entity, _Employees[entity.Id]);
            });
        }

        public async Task<ICollection<Employee>> WhereAsync(Func<Employee, bool> predicate) {
            return await Task.FromResult(_Employees.Values.Where(predicate).ToList());
        }
    }
}

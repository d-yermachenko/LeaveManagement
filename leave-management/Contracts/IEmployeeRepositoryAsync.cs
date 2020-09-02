using LeaveManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts {
    public interface IEmployeeRepositoryAsync: IRepositoryBaseAsync<Employee, string> {
        public Task<bool> RegisterEmployeeAsync(Employee employee, string password);
    }
}

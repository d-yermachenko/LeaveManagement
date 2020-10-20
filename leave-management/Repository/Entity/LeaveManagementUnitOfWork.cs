using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Repository.Entity {
    public class LeaveManagementUnitOfWork : ILeaveManagementUnitOfWork, IDisposable {

        private readonly ApplicationDbContext _ApplicationDbContext;
        private IRepository<LeaveType> _LeaveTypesRepository = null;
        private IRepository<LeaveAllocation> _LeaveAllocationsRepository = null;
        private IRepository<LeaveRequest> _LeaveRequestRepository = null;
        private IRepository<Employee> _EmployeesRepository = null;
        private IRepository<Company> _CompaniesRepository = null;

        public LeaveManagementUnitOfWork(ApplicationDbContext dbContext) {
            _ApplicationDbContext = dbContext;
        }


        public IRepository<LeaveType> LeaveTypes {
            get {
                if (_LeaveTypesRepository == null)
                    _LeaveTypesRepository = new GenericEntityRepository<LeaveType>(_ApplicationDbContext.Set<LeaveType>());
                return _LeaveTypesRepository;
            }
        }

        public IRepository<LeaveAllocation> LeaveAllocations {
            get {
                if (_LeaveAllocationsRepository == null)
                    _LeaveAllocationsRepository = new GenericEntityRepository<LeaveAllocation>(_ApplicationDbContext.Set<LeaveAllocation>());
                return _LeaveAllocationsRepository;
            }
        }

        public IRepository<LeaveRequest> LeaveRequest {
            get {
                if (_LeaveRequestRepository == null)
                    _LeaveRequestRepository = new GenericEntityRepository<LeaveRequest>(_ApplicationDbContext.Set<LeaveRequest>());
                return _LeaveRequestRepository;
            }
        }

        public IRepository<Employee> Employees {
            get {
                if (_EmployeesRepository == null)
                    _EmployeesRepository = new GenericEntityRepository<Employee>(_ApplicationDbContext.Set<Employee>());
                return _EmployeesRepository;
            }
        }

        public IRepository<Company> Companies {
            get {
                if (_CompaniesRepository == null)
                    _CompaniesRepository = new GenericEntityRepository<Company>(_ApplicationDbContext.Set<Company>());
                return _CompaniesRepository;
            }
        }

        public async Task<bool> Save() {
            bool result;
            try {
                int affectedRows = await _ApplicationDbContext.SaveChangesAsync();
                result = true;
            }
            catch {
                result = false;
                throw;
            }
            return result;
        }

        #region Disposing
        bool _Disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (_Disposed)
                return;

            if (disposing) {
                _ApplicationDbContext.Dispose();
            }

            _Disposed = true;

        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

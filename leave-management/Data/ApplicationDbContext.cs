using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Data.Entities;
using LeaveManagement.Contracts;
using LeaveManagement.Data.Adapters;

namespace LeaveManagement.Data {
    public class ApplicationDbContext : IdentityDbContext, IApplicationDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSets
        private DbSet<Employee> Employees { get; set; }

        private DbSet<LeaveType> LeaveTypes { get; set; }

        private DbSet<LeaveAllocation> LeaveAllocations { get; set; }

        private DbSet<LeaveHistory> LeaveHistories { get; set; }
        #endregion

        #region IDBContextImplementation
        private EntityDatabaseSet<Employee> _EmployeesData;

        public IDatabaseSet<Employee> EmployeesData {
            get {
                if (_EmployeesData == null)
                    _EmployeesData = new EntityDatabaseSet<Employee>(Employees);
                return _EmployeesData;
            }
        }

        private EntityDatabaseSet<LeaveType> _LeaveTypes;

        public IDatabaseSet<LeaveType> LeaveTypesData {
            get {
                if (_LeaveTypes == null)
                    _LeaveTypes = new EntityDatabaseSet<LeaveType>(LeaveTypes);
                return _LeaveTypes;
            }
        }

        private EntityDatabaseSet<LeaveAllocation> _LeaveAllocationsData;

        public IDatabaseSet<LeaveAllocation> LeaveAllocationsData {
            get {
                if (_LeaveAllocationsData == null)
                    _LeaveAllocationsData = new EntityDatabaseSet<LeaveAllocation>(LeaveAllocations);
                return _LeaveAllocationsData;
            }
        }


        private EntityDatabaseSet<LeaveHistory> _LeaveHistoriesData;

        public IDatabaseSet<LeaveHistory> LeaveHistoriesData {
            get {
                if (_LeaveHistoriesData == null)
                    _LeaveHistoriesData = new EntityDatabaseSet<LeaveHistory>(LeaveHistories);
                return _LeaveHistoriesData;
            }
        }

        #endregion

        public void Save()
        {
            this.SaveChanges();
        }
    }
}

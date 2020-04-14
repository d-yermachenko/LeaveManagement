using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Data.Entities;
using LeaveManagement.Contracts;
using LeaveManagement.Models.ViewModels;

namespace LeaveManagement.Data {
    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSets
        public DbSet<Employee> Employees { get; set; }

        public DbSet<LeaveType> LeaveTypes { get; set; }

        public DbSet<LeaveAllocation> LeaveAllocations { get; set; }

        public DbSet<LeaveHistory> LeaveHistories { get; set; }
        #endregion

        


        public bool Save()
        {
            return this.SaveChanges() > 0;
        }

        


        public DbSet<LeaveManagement.Models.ViewModels.LeaveTypeNavigationViewModel> LeaveTypeNavigationViewModel { get; set; }

        


        public DbSet<LeaveManagement.Models.ViewModels.LeaveTypeCreation> LeaveTypeCreation { get; set; }
    }
}

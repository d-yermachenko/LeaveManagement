using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Data.Entities;
using LeaveManagement.Contracts;
using System.Threading.Tasks;
using LeaveManagement.ViewModels.LeaveRequest;

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

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        public DbSet<Company> Companies { get; set; }
        #endregion

        


        public async Task<bool> Save()
        {
            return (await this.SaveChangesAsync()) > 0;
        }
   
     


    }
}

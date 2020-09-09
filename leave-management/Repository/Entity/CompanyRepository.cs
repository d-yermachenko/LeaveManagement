using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Repository.Entity {
    public class CompanyRepository : ICompanyRepository {
        private readonly ApplicationDbContext _DBContext;
        private readonly ILogger<CompanyRepository> _Logger;

        public CompanyRepository(
            ApplicationDbContext dbContext,
            ILogger<CompanyRepository> logger) {
            _DBContext = dbContext;
            _Logger = logger;

        }
        public async Task<bool> CreateAsync(Company entity) {
            _DBContext.Companies.Add(entity);
            return await SaveAsync();
        }

        public async Task<bool> DeleteAsync(Company entity) {
            _DBContext.Companies.Remove(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<Company>> FindAllAsync() {
            return await _DBContext.Companies.ToArrayAsync();
        }

        public async Task<Company> FindByIdAsync(int id) {
            return await _DBContext.Companies.FindAsync(id);
        }

        public async Task<bool> SaveAsync() {
            return (await _DBContext.SaveChangesAsync()) > 0;
        }

        public async Task<bool> UpdateAsync(Company entity) {
            _DBContext.Companies.Update(entity);
            return await SaveAsync();
        }

        public async Task<ICollection<Company>> WhereAsync(Func<Company, bool> predicate) {
            return await Task.Run<ICollection<Company>>(()=> { return _DBContext.Companies.Where(predicate).ToList(); });
        }
    }
}

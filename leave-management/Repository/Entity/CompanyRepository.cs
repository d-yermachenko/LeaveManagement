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
            bool result = false;
            try {
                _DBContext.Companies.Add(entity);
                result = await SaveAsync();
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;
        }

        public async Task<bool> DeleteAsync(Company entity) {
            bool result = false;
            try {
                _DBContext.Companies.Remove(entity);
                result = await SaveAsync();
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;
        }

        public async Task<ICollection<Company>> FindAllAsync() {
            return await _DBContext.Companies.ToArrayAsync();
        }

        public async Task<Company> FindByIdAsync(int id) {
            return await _DBContext.Companies.FindAsync(id);
        }

        public async Task<bool> SaveAsync() {
            bool result = false;
            try {
                result = (await _DBContext.SaveChangesAsync()) > 0;
            }
            catch (AggregateException ae) {
                ae.Flatten().InnerExceptions.ToList().ForEach((ie) => _Logger.LogError(ie, ie.Message, new object[] { }));
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;

        }

        public async Task<bool> UpdateAsync(Company entity) {
            bool result = false;
            try {
                _DBContext.Companies.Update(entity);
                result = await SaveAsync();
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;
        }

        public async Task<ICollection<Company>> WhereAsync(Func<Company, bool> predicate) {
            return await Task.Run<ICollection<Company>>(() => { return _DBContext.Companies.Where(predicate).ToList(); });
        }
    }
}

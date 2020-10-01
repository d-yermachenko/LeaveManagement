using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Data.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace LeaveManagement.Repository.Entity {
    public class LeaveTypeRepository : ILeaveTypeRepositoryAsync {

        ApplicationDbContext ApplicationDbContext;
        ILogger<LeaveTypeRepository> _Logger;


        public LeaveTypeRepository(ApplicationDbContext applicationDbContext,
             ILogger<LeaveTypeRepository> logger) {
            ApplicationDbContext = applicationDbContext;
            _Logger = logger;
        }

        public async Task<bool> CreateAsync(LeaveType entity) {
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypes.Add(entity);
                result = await SaveAsync();
            }
            catch (AggregateException ae) {
                var logableAE = ae.Flatten();
                _Logger.LogError(logableAE, logableAE.Message);
                throw;
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
                throw;
            }
            return result;
        }

        public async Task<bool> DeleteAsync(LeaveType entity) {
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypes.Remove(entity);
                result =  await SaveAsync();
            }
            catch (AggregateException ae) {
                var logableAE = ae.Flatten();
                _Logger.LogError(logableAE, logableAE.Message);
                throw;
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
                throw;
            }
            return result;
        }

        public async Task<ICollection<LeaveType>> FindAllAsync() => await Task.Run(()=> ApplicationDbContext.LeaveTypes.ToList());

        public async Task<LeaveType> FindByIdAsync(int id) {
            LeaveType result = null;
            try {
                result =  await ApplicationDbContext.LeaveTypes.FindAsync(id);
            }
            catch (AggregateException ae) {
                var logableAE = ae.Flatten();
                _Logger.LogError(logableAE, logableAE.Message);
                throw;
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
                throw;
            }
            return result;
        }


        public async Task<bool> SaveAsync() {
            bool result = false;
            try {
                result = (await ApplicationDbContext.SaveChangesAsync()) > 0;
                
            }
            catch(AggregateException ae) {
                var logableAE = ae.Flatten();
                _Logger.LogError(logableAE, logableAE.Message);
                throw;
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
                throw;
            }
            return result;
        }

        public async Task<bool> UpdateAsync(LeaveType entity) {
            bool result = false;
            try {
                ApplicationDbContext.LeaveTypes.Update(entity);
                result = await SaveAsync();
            }
            catch(AggregateException ae)  {
                var loggableAE = ae.Flatten();
                _Logger.LogError(loggableAE, loggableAE.Message);
            }
            catch(Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;
        }

        public async Task<ICollection<LeaveType>> WhereAsync(Func<LeaveType, bool> predicate) {
            ICollection<LeaveType> result = new LeaveType[] { };
            try {
                result = await Task.Run(() => ApplicationDbContext.LeaveTypes.Where(predicate)?.ToList());
            }
            catch (AggregateException ae) {
                var loggableAE = ae.Flatten();
                _Logger.LogError(loggableAE, loggableAE.Message);
            }
            catch (Exception e) {
                _Logger.LogError(e, e.Message);
            }
            return result;
        }
    }
}

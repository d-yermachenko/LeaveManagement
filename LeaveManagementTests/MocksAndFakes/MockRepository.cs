using LeaveManagement.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagementTests.MocksAndFakes {
    public abstract class MockRepository<TId, TInstance> : IRepository<TInstance>
        where TInstance : class {
        private ConcurrentDictionary<TId, TInstance> _MockData;
        private Func<TInstance, TId> _IdGetter;
        private object locker = new object();

        protected MockRepository(Expression<Func<TInstance, TId>> idAccessor) {
            _MockData = new ConcurrentDictionary<TId, TInstance>();
            _IdGetter = idAccessor.Compile();
        }

        public Task<bool> CreateAsync(TInstance entity) {
            return Task.Factory.StartNew<bool>(() => {
                TId id = _IdGetter.Invoke(entity);
                if (_MockData.ContainsKey(id))
                    return false;
                else
                    return _MockData.TryAdd(id, entity);
            });


        }

        public Task<bool> DeleteAsync(TInstance entity) {
            return Task.FromResult(_MockData.TryRemove(_IdGetter.Invoke(entity), out _));
        }

        public Task<TInstance> FindAsync(Expression<Func<TInstance, bool>> predicate, IEnumerable<Expression<Func<TInstance, object>>> includes = null) {
            return Task.Factory.StartNew(() => {
                IQueryable<TInstance> instances = _MockData.Values.AsQueryable();
                var requestedData = instances.Where(predicate);
                return requestedData.FirstOrDefault();
            });
        }

        public Task<bool> UpdateAsync(TInstance entity) {
            return Task.Factory.StartNew(() => {
                IQueryable<TInstance> instances = _MockData.Values.AsQueryable();
                lock (locker) {
                    TId id = _IdGetter.Invoke(entity);
                    var originalInstance = _MockData[id];
                    return _MockData.TryUpdate(id, entity, originalInstance);
                }
            });
        }

        public Task<ICollection<TInstance>> WhereAsync(Expression<Func<TInstance, bool>> filter = null, Func<IQueryable<TInstance>, IOrderedQueryable<TInstance>> order = null, IEnumerable<Expression<Func<TInstance, object>>> includes = null) {
            return Task.Factory.StartNew<ICollection<TInstance>>(() => {
                IQueryable<TInstance> instances = _MockData.Values.AsQueryable();
                var requestedData = instances.Where(filter);
                return requestedData.ToList();
            });
        }
    }
}

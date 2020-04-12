using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Data.Adapters {
    public class EntityDatabaseSet<TEntityType> : IDatabaseSet<TEntityType> where TEntityType : class{
        private readonly DbSet<TEntityType> InternalDbSet;

        public EntityDatabaseSet(DbSet<TEntityType> dbSet)
        {
            InternalDbSet = dbSet;
        }
        #region ImplementationOfParentInterfaces

        public Type ElementType => typeof(TEntityType);

        public Expression Expression => ((IQueryable)InternalDbSet).Expression;

        public IQueryProvider Provider => ((IQueryable)InternalDbSet).Provider;

        public bool ContainsListCollection => ((IListSource)InternalDbSet).ContainsListCollection;

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)InternalDbSet).GetEnumerator();
        }

        public IAsyncEnumerator<TEntityType> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return ((IAsyncEnumerable<TEntityType>)InternalDbSet).GetAsyncEnumerator(cancellationToken);
        }

        public IEnumerator<TEntityType> GetEnumerator() {
            return ((IEnumerable<TEntityType>)InternalDbSet).GetEnumerator();
        }

        public IList GetList() {
            return ((IListSource)InternalDbSet).GetList();
        }
        #endregion

        #region Implementation of general Methods

        public void Add(TEntityType entity) => InternalDbSet.Add(entity);
        public void AddRange(IEnumerable<TEntityType> entities) => InternalDbSet.AddRange(entities);
        public void AddRange(params TEntityType[] entities) => InternalDbSet.AddRange(entities);

        public Task AddRangeAsync(IEnumerable<TEntityType> entities, CancellationToken cancellationToken = default) =>
            InternalDbSet.AddRangeAsync(entities, cancellationToken);


        public Task AddRangeAsync(params TEntityType[] entities) => InternalDbSet.AddRangeAsync(entities);

        public TEntityType Find(params object[] keyValues) => InternalDbSet.Find(keyValues);

        public ValueTask<TEntityType> FindAsync(object[] keyValues, CancellationToken cancellationToken) =>
            InternalDbSet.FindAsync(keyValues, cancellationToken);

        public ValueTask<TEntityType> FindAsync(params object[] keyValues) => InternalDbSet.FindAsync(keyValues);

      

        public void Remove(TEntityType entity)
        {
            InternalDbSet.Remove(entity);
        }

        public void RemoveRange(params TEntityType[] entities)
        {
            InternalDbSet.RemoveRange(entities);
        }

        public void RemoveRange(IEnumerable<TEntityType> entities)
        {
            InternalDbSet.RemoveRange(entities);
        }

        public void UpdateRange(params TEntityType[] entities)
        {
            InternalDbSet.UpdateRange(entities);
        }

        public void UpdateRange(IEnumerable<TEntityType> entities)
        {
            InternalDbSet.UpdateRange(entities);
        }

        public void Update(TEntityType entity) {
            if (InternalDbSet.Contains(entity))
                InternalDbSet.Update(entity);
            else
                InternalDbSet.Add(entity);

        }
        #endregion


    }
}

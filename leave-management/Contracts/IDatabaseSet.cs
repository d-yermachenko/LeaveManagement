using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts {
    public interface IDatabaseSet<TEntity>: IQueryable<TEntity>, IAsyncEnumerable<TEntity>, IListSource where TEntity : class {

        void Add(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);
        void AddRange(params TEntity[] entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task AddRangeAsync(params TEntity[] entities);

        //
        // Summary:
        //     Finds an entity with the given primary key values. If an entity with the given
        //     primary key values is being tracked by the context, then it is returned immediately
        //     without making a request to the database. Otherwise, a query is made to the database
        //     for an entity with the given primary key values and this entity, if found, is
        //     attached to the context and returned. If no entity is found, then null is returned.
        //
        // Parameters:
        //   keyValues:
        //     The values of the primary key for the entity to be found.
        //
        // Returns:
        //     The entity found, or null.
        TEntity Find(params object[] keyValues);
        //
        // Summary:
        //     Finds an entity with the given primary key values. If an entity with the given
        //     primary key values is being tracked by the context, then it is returned immediately
        //     without making a request to the database. Otherwise, a query is made to the database
        //     for an entity with the given primary key values and this entity, if found, is
        //     attached to the context and returned. If no entity is found, then null is returned.
        //
        // Parameters:
        //   keyValues:
        //     The values of the primary key for the entity to be found.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task to
        //     complete.
        //
        // Returns:
        //     The entity found, or null.
        ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);
        //
        // Summary:
        //     Finds an entity with the given primary key values. If an entity with the given
        //     primary key values is being tracked by the context, then it is returned immediately
        //     without making a request to the database. Otherwise, a query is made to the database
        //     for an entity with the given primary key values and this entity, if found, is
        //     attached to the context and returned. If no entity is found, then null is returned.
        //
        // Parameters:
        //   keyValues:
        //     The values of the primary key for the entity to be found.
        //
        // Returns:
        //     The entity found, or null.
        ValueTask<TEntity> FindAsync(params object[] keyValues);

        //
        // Summary:
        //     Begins tracking the given entity in the Microsoft.EntityFrameworkCore.EntityState.Deleted
        //     state such that it will be removed from the database when Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     is called.
        //
        // Parameters:
        //   entity:
        //     The entity to remove.
        //
        // Returns:
        //     The Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1 for the entity.
        //     The entry provides access to change tracking information and operations for the
        //     entity.
        //
        // Remarks:
        //     If the entity is already tracked in the Microsoft.EntityFrameworkCore.EntityState.Added
        //     state then the context will stop tracking the entity (rather than marking it
        //     as Microsoft.EntityFrameworkCore.EntityState.Deleted) since the entity was previously
        //     added to the context and does not exist in the database.
        //     Any other reachable entities that are not already being tracked will be tracked
        //     in the same way that they would be if Microsoft.EntityFrameworkCore.DbSet`1.Attach(`0)
        //     was called before calling this method. This allows any cascading actions to be
        //     applied when Microsoft.EntityFrameworkCore.DbContext.SaveChanges is called.
        //     Use Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State to set the
        //     state of only a single entity.
        void Remove(TEntity entity);
        //
        // Summary:
        //     Begins tracking the given entities in the Microsoft.EntityFrameworkCore.EntityState.Deleted
        //     state such that they will be removed from the database when Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     is called.
        //
        // Parameters:
        //   entities:
        //     The entities to remove.
        //
        // Remarks:
        //     If any of the entities are already tracked in the Microsoft.EntityFrameworkCore.EntityState.Added
        //     state then the context will stop tracking those entities (rather than marking
        //     them as Microsoft.EntityFrameworkCore.EntityState.Deleted) since those entities
        //     were previously added to the context and do not exist in the database.
        //     Any other reachable entities that are not already being tracked will be tracked
        //     in the same way that they would be if Microsoft.EntityFrameworkCore.DbSet`1.AttachRange(`0[])
        //     was called before calling this method. This allows any cascading actions to be
        //     applied when Microsoft.EntityFrameworkCore.DbContext.SaveChanges is called.
       void RemoveRange(params TEntity[] entities);
        //
        // Summary:
        //     Begins tracking the given entities in the Microsoft.EntityFrameworkCore.EntityState.Deleted
        //     state such that they will be removed from the database when Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     is called.
        //
        // Parameters:
        //   entities:
        //     The entities to remove.
        //
        // Remarks:
        //     If any of the entities are already tracked in the Microsoft.EntityFrameworkCore.EntityState.Added
        //     state then the context will stop tracking those entities (rather than marking
        //     them as Microsoft.EntityFrameworkCore.EntityState.Deleted) since those entities
        //     were previously added to the context and do not exist in the database.
        //     Any other reachable entities that are not already being tracked will be tracked
        //     in the same way that they would be if Microsoft.EntityFrameworkCore.DbSet`1.AttachRange(System.Collections.Generic.IEnumerable{`0})
        //     was called before calling this method. This allows any cascading actions to be
        //     applied when Microsoft.EntityFrameworkCore.DbContext.SaveChanges is called.
        void RemoveRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        //
        // Summary:
        //     Begins tracking the given entities and entries reachable from the given entities
        //     using the Microsoft.EntityFrameworkCore.EntityState.Modified state by default,
        //     but see below for cases when a different state will be used.
        //     Generally, no database interaction will be performed until Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     is called.
        //     A recursive search of the navigation properties will be performed to find reachable
        //     entities that are not already being tracked by the context. All entities found
        //     will be tracked by the context.
        //     For entity types with generated keys if an entity has its primary key value set
        //     then it will be tracked in the Microsoft.EntityFrameworkCore.EntityState.Modified
        //     state. If the primary key value is not set then it will be tracked in the Microsoft.EntityFrameworkCore.EntityState.Added
        //     state. This helps ensure new entities will be inserted, while existing entities
        //     will be updated. An entity is considered to have its primary key value set if
        //     the primary key property is set to anything other than the CLR default for the
        //     property type.
        //     For entity types without generated keys, the state set is always Microsoft.EntityFrameworkCore.EntityState.Modified.
        //     Use Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State to set the
        //     state of only a single entity.
        //
        // Parameters:
        //   entities:
        //     The entities to update.
       void UpdateRange(params TEntity[] entities);
        //
        // Summary:
        //     Begins tracking the given entities and entries reachable from the given entities
        //     using the Microsoft.EntityFrameworkCore.EntityState.Modified state by default,
        //     but see below for cases when a different state will be used.
        //     Generally, no database interaction will be performed until Microsoft.EntityFrameworkCore.DbContext.SaveChanges
        //     is called.
        //     A recursive search of the navigation properties will be performed to find reachable
        //     entities that are not already being tracked by the context. All entities found
        //     will be tracked by the context.
        //     For entity types with generated keys if an entity has its primary key value set
        //     then it will be tracked in the Microsoft.EntityFrameworkCore.EntityState.Modified
        //     state. If the primary key value is not set then it will be tracked in the Microsoft.EntityFrameworkCore.EntityState.Added
        //     state. This helps ensure new entities will be inserted, while existing entities
        //     will be updated. An entity is considered to have its primary key value set if
        //     the primary key property is set to anything other than the CLR default for the
        //     property type.
        //     For entity types without generated keys, the state set is always Microsoft.EntityFrameworkCore.EntityState.Modified.
        //     Use Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State to set the
        //     state of only a single entity.
        //
        // Parameters:
        //   entities:
        //     The entities to update.
        void UpdateRange(IEnumerable<TEntity> entities);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveManagement.Contracts {
    public interface IRepositoryBaseAsync<T, I>  where T :class
                                     {
         Task<ICollection<T>> FindAllAsync();

         Task<T> FindByIdAsync(I id);

         Task<bool> CreateAsync(T entity);

         Task<bool> UpdateAsync(T entity);

         Task<bool> DeleteAsync(T entity);

         Task<bool> SaveAsync();
    }
}

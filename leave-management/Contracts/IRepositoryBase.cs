using System.Collections.Generic;

namespace LeaveManagement.Contracts {
    public interface IRepositoryBase<T, I>  where T :class
                                     where I : struct{
        ICollection<T> FindAll();

        T FindById(I id);

        bool Create(T entity);

        bool Update(T entity);

        bool Delete(T entity);

        bool Save();
    }
}

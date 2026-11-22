using System.Linq.Expressions;
using BlogAPI.Core.Entities;

namespace BlogAPI.Data.Repositories.Abstractions;

public interface IRepository<T> where T : class, IEntityBase
{
    Task AddAsync(T entity);

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includeProperties);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
    void Update(T entity);
    void Delete(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

using System.Linq.Expressions;
using BlogAPI.Core.Entities;
using BlogAPI.Data.Context;
using BlogAPI.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Data.Repositories.Concretes;

public class Repository<T> : IRepository<T> where T : class, IEntityBase
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    private DbSet<T> Table => _context.Set<T>();
    
    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] includeProperties)
    {
        IQueryable<T> query = ApplyIncludes(Table.AsNoTracking(), includeProperties);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
    {
        return ApplyIncludes(Table, includeProperties).FirstOrDefaultAsync(predicate);
    }

    public void Update(T entity)
    {
        Table.Update(entity);
    }

    public void Delete(T entity)
    {
        Table.Remove(entity);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await Table.FindAsync(id);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return Table.AnyAsync(predicate);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate is null ? Table.CountAsync() : Table.CountAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await Table.AddAsync(entity);
    }

    private static IQueryable<T> ApplyIncludes(
        IQueryable<T> query, IEnumerable<Expression<Func<T, object>>> includeProperties)
    {
        foreach (var include in includeProperties)
        {
            query = query.Include(include);
        }

        return query;
    }
}

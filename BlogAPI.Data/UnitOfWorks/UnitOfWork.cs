using BlogAPI.Core.Entities;
using BlogAPI.Data.Context;
using BlogAPI.Data.Repositories.Abstractions;
using BlogAPI.Data.Repositories.Concretes;

namespace BlogAPI.Data.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> GetRepository<T>() where T : class, IEntityBase
        => new Repository<T>(_context);

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}

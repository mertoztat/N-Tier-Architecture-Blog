using BlogAPI.Core.Entities;
using BlogAPI.Data.Repositories.Abstractions;

namespace BlogAPI.Data.UnitOfWorks;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> GetRepository<T>() where T : class, IEntityBase;
    Task<int> SaveAsync();
}

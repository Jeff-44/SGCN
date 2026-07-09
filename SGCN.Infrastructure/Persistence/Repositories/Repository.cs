using Microsoft.EntityFrameworkCore;
using SGCN.Application.Interfaces;
using SGCN.Domain.Common;

namespace SGCN.Infrastructure.Persistence.Repositories;

public sealed class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Set<T>()
            .FirstOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken);
    }

    public IQueryable<T> Query()
    {
        return _context.Set<T>().Where(entity => !entity.IsDeleted);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Set<T>().Update(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Data;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly SavingsStoreContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(SavingsStoreContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Data;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Repositories;

public class GoalRepository : Repository<Goal>, IGoalRepository
{
    public GoalRepository(SavingsStoreContext context) : base(context) { }

    public async Task<IEnumerable<Goal>> GetGoalsWithDetailsAsync(Guid userId) =>
        await _dbSet
            .Where(g => g.CreatedBy == userId)
            .Include(g => g.User)
            .Include(g => g.Deposits)
            .Include(g => g.Withdrawals)
            .AsSplitQuery()
            .ToListAsync();

    public async Task<Goal?> GetGoalWithDetailsAsync(Guid id) =>
        await _dbSet
            .Include(g => g.User)
            .Include(g => g.Deposits)
            .Include(g => g.Withdrawals)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> ExistsForUserAsync(Guid goalId, Guid userId) =>
        await _dbSet.AnyAsync(g => g.Id == goalId && g.CreatedBy == userId);
}
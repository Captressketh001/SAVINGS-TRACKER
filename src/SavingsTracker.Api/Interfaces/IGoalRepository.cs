using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Interfaces;

public interface IGoalRepository : IRepository<Goal>
{
    Task<IEnumerable<Goal>> GetGoalsWithDetailsAsync(Guid userId);
    Task<Goal?> GetGoalWithDetailsAsync(Guid id);
    Task<bool> ExistsForUserAsync(Guid goalId, Guid userId);
}
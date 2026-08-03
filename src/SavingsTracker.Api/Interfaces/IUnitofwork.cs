using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Interfaces;

public interface IUnitOfWork
{
    IGoalRepository Goals { get; }
    IRepository<Deposit> Deposits { get; }
    IRepository<Withdrawal> Withdrawals { get; }
    IRepository<User> Users { get; }
    Task<int> SaveChangesAsync();
}
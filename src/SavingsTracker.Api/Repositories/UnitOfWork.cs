using SavingsTracker.Api.Data;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SavingsStoreContext _context;

    public IGoalRepository Goals { get; }
    public IRepository<Deposit> Deposits { get; }
    public IRepository<Withdrawal> Withdrawals { get; }
    
    public IRepository<User> Users { get; }
    
    public UnitOfWork(SavingsStoreContext context)
    {
        _context = context;
        Goals = new GoalRepository(context);
        Deposits = new Repository<Deposit>(context);
        Withdrawals = new Repository<Withdrawal>(context);
        Users = new Repository<User>(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
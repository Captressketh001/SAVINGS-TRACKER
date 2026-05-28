namespace SavingsTracker.Api.Models;

public class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public decimal TargetAmount { get; set; }
    
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Deposit> Deposits {get; set;} = [];

    public ICollection<Withdrawal> Withdrawals {get; set;} = [];
}
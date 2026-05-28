namespace SavingsTracker.Api.Models;

public class Withdrawal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public Guid GoalId { get; set; }
    public Goal Goal { get; set; } = null!;
}
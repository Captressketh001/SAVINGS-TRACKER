namespace SavingsTracker.Api.Dtos;

public record DepositDetailDto
(
    Guid Id,
    Guid GoalId,
    decimal Amount,
    string? Note,
    DateTime Date
);
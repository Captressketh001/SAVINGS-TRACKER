namespace SavingsTracker.Api.Dtos;

public record GoalsDetailDto
(
    Guid Id,
    string Name,
    string Username,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Status,
    DateTime? Deadline,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
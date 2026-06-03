namespace SavingsTracker.Api.Dtos;

public record UpdateGoalDto
(
    string? Name,
    decimal? TargetAmount,
    DateTime? Deadline
);
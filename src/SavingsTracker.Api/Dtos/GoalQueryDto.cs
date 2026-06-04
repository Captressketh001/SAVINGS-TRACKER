namespace SavingsTracker.Api.Dtos;

public record GoalQueryDto
(
    string? Status,
    string? SortBy,
    string? SortOrder
);
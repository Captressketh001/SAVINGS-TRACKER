namespace SavingsTracker.Api.Dtos;

public record GoalSummaryDto
(
    decimal TotalSavings,
    int ActiveGoals,
    int CompletedGoals
);
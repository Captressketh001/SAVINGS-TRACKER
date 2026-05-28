using System.ComponentModel.DataAnnotations;

namespace SavingsTracker.Api.Dtos;

public record CreateGoalDto
(
    [Required] string Name,
    [Required] [Range(1, double.MaxValue)] decimal TargetAmount,
    DateTime? Deadline
);
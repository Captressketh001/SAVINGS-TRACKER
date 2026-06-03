using System.ComponentModel.DataAnnotations;

namespace SavingsTracker.Api.Dtos;

public record WithdrawDto
(
    [Required] [Range(1, double.MaxValue)] decimal Amount,
    string? Note
);
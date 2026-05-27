using System.ComponentModel.DataAnnotations;

namespace SavingsTracker.Api.Dtos;

public record Login
(
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);

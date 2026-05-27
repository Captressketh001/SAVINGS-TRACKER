using System.ComponentModel.DataAnnotations;

namespace SavingsTracker.Api.Dtos;

public record SignUp
(
    [Required][MaxLength(50)] string Username,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);
   
  
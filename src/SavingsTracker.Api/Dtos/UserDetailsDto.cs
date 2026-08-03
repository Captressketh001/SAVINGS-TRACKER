namespace SavingsTracker.Api.Dtos;

public record UserDetailDto(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt
);
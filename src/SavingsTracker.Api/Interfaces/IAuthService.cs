using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;

namespace SavingsTracker.Api.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> Register(SignUp dto);
    Task<ApiResponse<AuthResponse>> Login(Login dto, HttpContext http);
    Task<ApiResponse<AuthResponse>> RefreshToken(HttpContext http);
    Task<ApiResponse<string>> Logout(HttpContext http);
}
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;

namespace SavingsTracker.Api.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> Register(SignUp dto);
    Task<ApiResponse<string?>> Login(Login dto, HttpContext http);
    Task<ApiResponse<string?>> RefreshToken(HttpContext http);
    Task<ApiResponse<string>> Logout(HttpContext http);

    Task<ApiResponse<UserDetailDto>> UserDetails(HttpContext http);
}
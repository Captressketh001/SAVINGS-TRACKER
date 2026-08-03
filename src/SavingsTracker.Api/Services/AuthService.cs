using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class AuthService : IAuthService
{

    private readonly IUnitOfWork _unitOfWork;
    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> Register(SignUp dto)
    {
        var existingUser = await _unitOfWork.Users
            .FindAsync(u => u.Email == dto.Email);

        if (existingUser.Any())
            return new ApiResponse<string>(
                ResponseMsg: "Email already in use",
                ResponseDetails: null,
                ResponseCode: 400
            );

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return new ApiResponse<string>(
            ResponseMsg: "User registered successfully",
            ResponseDetails: null,
            ResponseCode: 201
        );
    }

    public async Task<ApiResponse<string?>> Login(Login dto, HttpContext http)
    {
        var users = await _unitOfWork.Users
            .FindAsync(u => u.Email == dto.Email);

        var user = users.FirstOrDefault();
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new ApiResponse<string?>(
                ResponseMsg: "Invalid email or password",
                ResponseDetails: null,
                ResponseCode: 401
        );

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        http.Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(
            double.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")!))
        });
        http.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });
        return new ApiResponse<string?>(
            ResponseMsg: "Login successful",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<string?>> RefreshToken(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return new ApiResponse<string?>(
                ResponseMsg: "Refresh token not found",
                ResponseDetails: null,
                ResponseCode: 401
            );

        var users = await _unitOfWork.Users
            .FindAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        var user = users.FirstOrDefault();
        if (user == null)
            return new ApiResponse<string?>(
                ResponseMsg: "Invalid or expired refresh token",
                ResponseDetails: null,
                ResponseCode: 401
            );

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await _unitOfWork.SaveChangesAsync();


        http.Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(
                    double.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")!))
        });
        http.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        return new ApiResponse<string?>(
            ResponseMsg: "Token refreshed successfully",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<string>> Logout(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return new ApiResponse<string>(
                ResponseMsg: "Refresh token not found",
                ResponseDetails: null,
                ResponseCode: 401
            );

        var users = await _unitOfWork.Users
            .FindAsync(u => u.RefreshToken == refreshToken);

        var user = users.FirstOrDefault();
        if (user == null)
            return new ApiResponse<string>(
                ResponseMsg: "Invalid token",
                ResponseDetails: null,
                ResponseCode: 401
            );

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _unitOfWork.SaveChangesAsync();

        http.Response.Cookies.Delete("refreshToken");
        http.Response.Cookies.Delete("accessToken");
        return new ApiResponse<string>(
            ResponseMsg: "Logout successful",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<UserDetailDto>> UserDetails(HttpContext http)
{
    var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userIdClaim))
        return new ApiResponse<UserDetailDto>(
            ResponseMsg: "User not authenticated",
            ResponseDetails: null,
            ResponseCode: 401
        );

    var userId = Guid.Parse(userIdClaim);
    var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
    var user = users.FirstOrDefault();

    if (user is null)
        return new ApiResponse<UserDetailDto>(
            ResponseMsg: "User not found",
            ResponseDetails: null,
            ResponseCode: 404
        );

    return new ApiResponse<UserDetailDto>(
        ResponseMsg: "User retrieved successfully",
        ResponseDetails: new UserDetailDto(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt
        ),
        ResponseCode: 200
    );
}
    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

}
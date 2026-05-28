using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SavingsTracker.Api.Data;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class AuthService : IAuthService
{
    private readonly SavingsStoreContext _context;


    public AuthService(SavingsStoreContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Register(SignUp dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if(existingUser != null)
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

        _context.Users.Add(user);
        await  _context.SaveChangesAsync();
        return new ApiResponse<string>(
            ResponseMsg: "User registered successfully",
            ResponseDetails: null,
            ResponseCode: 201
        );
    }

    public async Task<ApiResponse<AuthResponse>> Login(Login dto, HttpContext http)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new ApiResponse<AuthResponse>(
                ResponseMsg: "Invalid email or password",
                ResponseDetails: null,
                ResponseCode: 401
        );

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        http.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });
        return new ApiResponse<AuthResponse>(
            ResponseMsg: "Login successful",
            ResponseDetails: new AuthResponse(newAccessToken),
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<AuthResponse>> RefreshToken(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return new ApiResponse<AuthResponse>(
                ResponseMsg: "Refresh token not found",
                ResponseDetails: null,
                ResponseCode: 401
            );

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return new ApiResponse<AuthResponse>(
                ResponseMsg: "Invalid or expired refresh token",
                ResponseDetails: null,
                ResponseCode: 401
            );

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await _context.SaveChangesAsync();

        http.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        return new ApiResponse<AuthResponse>(
            ResponseMsg: "Token refreshed successfully",
            ResponseDetails: new AuthResponse(newAccessToken),
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

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null)
            return new ApiResponse<string>(
                ResponseMsg: "Invalid token",
                ResponseDetails: null,
                ResponseCode: 401
            );

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();

        http.Response.Cookies.Delete("refreshToken");

        return new ApiResponse<string>(
            ResponseMsg: "Logout successful",
            ResponseDetails: null,
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
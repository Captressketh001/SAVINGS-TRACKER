using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SavingsTracker.Api.Extensions;

public static class AuthExtension
{
    public static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY")!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
        var expiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")!);
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key))
                };
            });

        builder.Services.AddAuthorization();
    }
}
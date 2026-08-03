using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        const string EndpointName = "Auth";

        var group = app.MapGroup("/api/auth").WithTags(EndpointName);

        group.MapPost("/register", async (SignUp dto, IAuthService authService) =>
        {
            var result = await authService.Register(dto);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (Login dto, IAuthService authService, HttpContext http) =>
        {
            var result = await authService.Login(dto, http);
            return Results.Ok(result);
        });

        group.MapPost("/refresh-token", async (HttpContext http, IAuthService authService) =>
        {
            var result = await authService.RefreshToken(http);
            return Results.Ok(result);
        });

        group.MapPost("/logout", async (HttpContext http, IAuthService authService) =>
        {
            var result = await authService.Logout(http);
            return Results.Ok(result);
        });
        app.MapGet("/user/profile", async (HttpContext http, IAuthService authService) =>
        {
            var result = await authService.UserDetails(http);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
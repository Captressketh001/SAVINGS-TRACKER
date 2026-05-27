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

         group.MapPost("/login", async (Login dto, IAuthService authService) =>
         {
              var result = await authService.Login(dto);
              return Results.Ok(result);
         });
    }
}
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Endpoints;

public static class DepositEndpoint
{
    public static void MapDepositEndpoints (this WebApplication app)
    {
        const string EndpointName = "Deposit";

        var group = app.MapGroup("/api/goals")
                       .WithTags(EndpointName)
                       .RequireAuthorization();

        group.MapPost("/{id}/deposit", async (Guid id, DepositDto dto, IDepositService depositService) =>
        {
            var result = await depositService.AddDepositToGoal(id, dto);
            return Results.Ok(result);
        });
        group.MapGet("/{id}/deposits", async (Guid id, IDepositService depositService) =>
        {
            var result = await depositService.ListGoalDeposit(id);
            return Results.Ok(result);
        });
    }
}
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Endpoints;

public static class WithdrawEndpoint
{
    public static void MapWithdrawEndpoints (this WebApplication app)
    {
        const string EndpointName = "Withdraw";

        var group = app.MapGroup("/api/goals")
                       .WithTags(EndpointName)
                       .RequireAuthorization();

        group.MapPost("/{id}/withdraw", async (Guid id, WithdrawDto dto, IWithdrawService withdrawService) =>
        {
            var result = await withdrawService.WithdrawFromGoal(id, dto);
            return Results.Ok(result);
        });
        group.MapGet("/{id}/withdrawals", async (Guid id, IWithdrawService withdrawService) =>
        {
            var result = await withdrawService.ListGoalWithdrawal(id);
            return Results.Ok(result);
        });
    }
}
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Endpoints;

public static class GoalEndpoint
{
    public static void MapGoalEndpoints (this WebApplication app)
    {
        const string EndpointName = "Goals";

        var group = app.MapGroup("/api/goals").WithTags(EndpointName);

        group.MapPost("/create", async (CreateGoalDto dto, IGoalService goalService) =>
        {
            var result = await goalService.CreateGoal(dto);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
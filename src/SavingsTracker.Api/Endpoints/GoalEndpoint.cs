using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.Interfaces;

namespace SavingsTracker.Api.Endpoints;

public static class GoalEndpoint
{
    public static void MapGoalEndpoints (this WebApplication app)
    {
        const string EndpointName = "Goals";

        var group = app.MapGroup("/api/goals")
                       .WithTags(EndpointName)
                       .RequireAuthorization();

        group.MapGet("/", async (IGoalService goalService) =>
        {
            var result = await goalService.ListGoals();
            return Results.Ok(result);
        });
        group.MapGet("/{id}", async (Guid id, IGoalService goalService) =>
        {
            var result = await goalService.ListGoalById(id);
            return Results.Ok(result);
        });
        group.MapPost("/", async (CreateGoalDto dto, IGoalService goalService) =>
        {
            var result = await goalService.CreateGoal(dto);
            return Results.Ok(result);
        });

        group.MapPut("/{id}", async (Guid id, UpdateGoalDto dto, IGoalService goalService) =>
        {
            var result = await goalService.UpdateGoal(id, dto);
            return Results.Ok(result);
        });
        group.MapDelete("/{id}", async (Guid id, IGoalService goalService) =>
        {
            var result = await goalService.DeleteGoal(id);
            return Results.Ok(result);
        });
    }
}
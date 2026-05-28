using System.Security.Claims;
using SavingsTracker.Api.Data;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class GoalService : IGoalService
{
    private readonly SavingsStoreContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GoalService(SavingsStoreContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetLoggedInUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claim))
            throw new UnauthorizedAccessException("User not authenticated");

        return Guid.Parse(claim);
    }

    public async Task<ApiResponse<string>> CreateGoal(CreateGoalDto dto)
    {
        var userId = GetLoggedInUserId();
        var goal = new Goal
        {
            Name = dto.Name,
            TargetAmount = dto.TargetAmount,
            Deadline = dto.Deadline.HasValue 
                ? DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc)
                : null,
            CreatedBy = userId
        };

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        return new ApiResponse<string>
        (
            ResponseMsg: "Goal created successfully",
            ResponseDetails: null,
            ResponseCode: 201
        );
    }
}
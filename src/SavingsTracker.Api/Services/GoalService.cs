using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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

    public async Task<ApiResponse<IEnumerable<GoalsDetailDto>>> ListGoals()
    {
        var userId = GetLoggedInUserId();

        var goals = await _context.Goals
            .Where(g => g.CreatedBy == userId)
            .Include(g => g.User)
            .Include(g => g.Deposits)
            .Include(g => g.Withdrawals)
            .ToListAsync();

        if (!goals.Any())
            return new ApiResponse<IEnumerable<GoalsDetailDto>>(
                ResponseMsg: "No goals found",
                ResponseDetails: Enumerable.Empty<GoalsDetailDto>(),
                ResponseCode: 200
            );

        var goalDtos = goals.Select(g => new GoalsDetailDto(
            g.Id,
            g.Name,
            g.User.Username,
            g.TargetAmount,
            g.Deposits.Sum(d => d.Amount) - g.Withdrawals.Sum(w => w.Amount),
            g.Deposits.Sum(d => d.Amount) - g.Withdrawals.Sum(w => w.Amount) >= g.TargetAmount
                ? "Completed" : "Active",
            g.Deadline,
            g.CreatedAt,
            g.UpdatedAt
        ));

        return new ApiResponse<IEnumerable<GoalsDetailDto>>(
            ResponseMsg: "Goals retrieved successfully",
            ResponseDetails: goalDtos,
            ResponseCode: 200
        );

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

    public async Task<ApiResponse<string>> UpdateGoal(Guid id, UpdateGoalDto dto)
    {
        var userId = GetLoggedInUserId();
        var goal = await _context.Goals.FindAsync(id);

        if (goal is null)
        {
            return new ApiResponse<string>(
                ResponseMsg: "Goal does not exist",
                ResponseDetails: null,
                ResponseCode: 404
            );
        }

        if (goal.CreatedBy != userId)
        {
            return new ApiResponse<string>(
                ResponseMsg: "You are not authorized to edit this goal",
                ResponseDetails: null,
                ResponseCode: 403
            );
        }
        goal.Name = dto.Name ?? goal.Name;
        goal.Deadline = dto.Deadline ?? goal.Deadline;
        goal.TargetAmount = dto.TargetAmount ?? goal.TargetAmount;
        goal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ApiResponse<string>(
            ResponseMsg: "Goal Updated Successfully!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<string>> DeleteGoal(Guid id)
    {
        var userId = GetLoggedInUserId();
        var goal = await _context.Goals.FindAsync(id);

        if (goal is null)
        {
            return new ApiResponse<string>(
                ResponseMsg: "Goal does not exist",
                ResponseDetails: null,
                ResponseCode: 404
            );
        }

        if (goal.CreatedBy != userId)
        {
            return new ApiResponse<string>(
                ResponseMsg: "You are not authorized to delete this goal",
                ResponseDetails: null,
                ResponseCode: 403
            );
        }
        

        _context.Goals.Remove(goal);
        await _context.SaveChangesAsync();
        return new ApiResponse<string>(
            ResponseMsg: "Goal Deleted Successfully!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<GoalsDetailDto>> ListGoalById(Guid id)
    {
        var userId = GetLoggedInUserId();
        var goal = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.Deposits)
            .Include(g => g.Withdrawals)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (goal is null)
        {
            return new ApiResponse<GoalsDetailDto>(
                ResponseMsg: "Goal does not exist",
                ResponseDetails: null,
                ResponseCode: 404
            );
        }

        if (goal.CreatedBy != userId)
        {
            return new ApiResponse<GoalsDetailDto>(
                ResponseMsg: "You are not authorized to delete this goal",
                ResponseDetails: null,
                ResponseCode: 403
            );
        }

        var goalDtos = new GoalsDetailDto(
        goal.Id,
        goal.Name,
        goal.User.Username,
        goal.TargetAmount,
        goal.Deposits.Sum(d => d.Amount) - goal.Withdrawals.Sum(w => w.Amount),
        goal.Deposits.Sum(d => d.Amount) - goal.Withdrawals.Sum(w => w.Amount) >= goal.TargetAmount
            ? "Completed" : "Active",
        goal.Deadline,
        goal.CreatedAt,
        goal.UpdatedAt
    );
        
        return new ApiResponse<GoalsDetailDto>(
            ResponseMsg: "Goal Retrieved Successfully!",
            ResponseDetails: goalDtos,
            ResponseCode: 200
        );
    }
}
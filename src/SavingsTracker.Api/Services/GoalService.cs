using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class GoalService : IGoalService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IHttpContextAccessor _httpContextAccessor;


    public GoalService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
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

    public async Task<ApiResponse<IEnumerable<GoalsDetailDto>>> ListGoals(GoalQueryDto query)
    {
        var userId = GetLoggedInUserId();

        var goals = await _unitOfWork.Goals.GetGoalsWithDetailsAsync(userId);

        if (!goals.Any())
            return new ApiResponse<IEnumerable<GoalsDetailDto>>(
                ResponseMsg: "No goals found",
                ResponseDetails: Enumerable.Empty<GoalsDetailDto>(),
                ResponseCode: 200
            );


        var goalDtos = goals.Select(g =>
        {
            var currentAmount = g.Deposits.Sum(d => d.Amount) - g.Withdrawals.Sum(w => w.Amount);
            var status = currentAmount >= g.TargetAmount ? "Completed" : "Active";

            return new GoalsDetailDto(
                g.Id,
                g.Name,
                g.User.Username,
                g.TargetAmount,
                currentAmount,
                status,
                g.Deadline,
                g.CreatedAt,
                g.UpdatedAt
            );
        });

        if (!string.IsNullOrEmpty(query.Status))
            goalDtos = goalDtos.Where(g =>
                g.Status.Equals(query.Status?.ToLower(), StringComparison.OrdinalIgnoreCase));

        var sorted = query.SortBy switch
        {
            "name" => query.SortOrder?.ToLower() == "desc"
                         ? goalDtos.OrderByDescending(g => g.Name)
                         : goalDtos.OrderBy(g => g.Name),
            "deadline" => query.SortOrder == "desc"
                          ? goalDtos.OrderByDescending(g => g.Deadline.HasValue ? 0 : 1)
                                 .ThenByDescending(g => g.Deadline)
                          : goalDtos.OrderBy(g => g.Deadline.HasValue ? 0 : 1)
                                  .ThenBy(g => g.Deadline),
            "progress" => query.SortOrder == "desc"
                        ? goalDtos.OrderByDescending(g => g.CurrentAmount / g.TargetAmount)
                        : goalDtos.OrderBy(g => g.CurrentAmount / g.TargetAmount),
            "amountsaved" => query.SortOrder == "desc"
                            ? goalDtos.OrderByDescending(g => g.CurrentAmount)
                            : goalDtos.OrderBy(g => g.CurrentAmount),
            _ => goalDtos

        };
        return new ApiResponse<IEnumerable<GoalsDetailDto>>(
            ResponseMsg: "Goals retrieved successfully",
            ResponseDetails: sorted,
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

        await _unitOfWork.Goals.AddAsync(goal);
        await _unitOfWork.SaveChangesAsync();

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
        var goal = await _unitOfWork.Goals.GetByIdAsync(id);

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

        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<string>(
            ResponseMsg: "Goal Updated Successfully!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<string>> DeleteGoal(Guid id)
    {
        var userId = GetLoggedInUserId();
        var goal = await _unitOfWork.Goals.GetByIdAsync(id);

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


        _unitOfWork.Goals.Remove(goal);
        await _unitOfWork.SaveChangesAsync();
        return new ApiResponse<string>(
            ResponseMsg: "Goal Deleted Successfully!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<GoalsDetailDto>> ListGoalById(Guid id)
    {
        var userId = GetLoggedInUserId();
        var goal = await _unitOfWork.Goals.GetGoalWithDetailsAsync(id);

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

    public async Task<ApiResponse<GoalSummaryDto>> GoalSummary()
    {
        var userId = GetLoggedInUserId();

        var goals = await _unitOfWork.Goals.GetGoalsWithDetailsAsync(userId);

        if (!goals.Any())
            return new ApiResponse<GoalSummaryDto>(
                ResponseMsg: "Goals Summary Retrieved successfully",
                ResponseDetails: new GoalSummaryDto(
                TotalSavings: 0,
                ActiveGoals: 0,
                CompletedGoals: 0
            ),
                ResponseCode: 200
            );

        static decimal GetCurrentAmount(Goal g) =>
            g.Deposits.Sum(d => d.Amount) - g.Withdrawals.Sum(w => w.Amount);

        var totalSavings = goals.Sum(GetCurrentAmount);
        var activeGoals = goals.Count(g => GetCurrentAmount(g) < g.TargetAmount);
        var completedGoals = goals.Count(g => GetCurrentAmount(g) >= g.TargetAmount);

        return new ApiResponse<GoalSummaryDto>(
            ResponseMsg: "Goals Summary Retrieved successfully",
            ResponseDetails: new GoalSummaryDto(
                totalSavings, activeGoals, completedGoals
            ),
            ResponseCode: 200
        );

    }
    public async Task<ApiResponse<IEnumerable<MonthlyDepositDto>>> GoalMonthlyDeposit(string range = "3months")
    {
        var userId = GetLoggedInUserId();

        DateTime? startDate = range switch
        {
            "1month" => DateTime.UtcNow.AddMonths(-1),
            "3months" => DateTime.UtcNow.AddMonths(-3),
            "6months" => DateTime.UtcNow.AddMonths(-6),
            "year" => new DateTime(DateTime.UtcNow.Year, 1, 1),
            "all" => null,
            _ => DateTime.UtcNow.AddMonths(-3)
        };

        var allDeposits = await _unitOfWork.Deposits
     .FindAsync(d => d.Goal.CreatedBy == userId &&
         (startDate == null || d.Date >= startDate));

        var deposits = allDeposits
            .Select(d => new { d.Date, d.Amount })
            .ToList();

        if (deposits.Count == 0)
        {
            return new ApiResponse<IEnumerable<MonthlyDepositDto>>(
                ResponseMsg: "No Deposit within this range",
                ResponseDetails: [],
                ResponseCode: 200
            );
        }

        var grouped = deposits
            .GroupBy(d => new { d.Date.Year, d.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyDepositDto(
                g.Key.Year,
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                g.Sum(d => d.Amount)
        ));

        return new ApiResponse<IEnumerable<MonthlyDepositDto>>(
            ResponseMsg: "Monthly deposits retrieved successfully",
            ResponseDetails: grouped,
            ResponseCode: 200
        );
    }
}
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;


namespace SavingsTracker.Api.Interfaces;

public interface IGoalService
{
    Task<ApiResponse<string>> CreateGoal(CreateGoalDto dto);

    Task<ApiResponse<IEnumerable<GoalsDetailDto>>> ListGoals(GoalQueryDto query);

    Task<ApiResponse<GoalsDetailDto>> ListGoalById(Guid id);

    Task<ApiResponse<string>> UpdateGoal(Guid id, UpdateGoalDto dto);

    Task<ApiResponse<string>> DeleteGoal(Guid id);

    Task <ApiResponse<GoalSummaryDto>> GoalSummary();

    Task <ApiResponse<IEnumerable<MonthlyDepositDto>>> GoalMonthlyDeposit(string range="3months");
}
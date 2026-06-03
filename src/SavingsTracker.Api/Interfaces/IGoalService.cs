using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;


namespace SavingsTracker.Api.Interfaces;

public interface IGoalService
{
    Task<ApiResponse<string>> CreateGoal(CreateGoalDto dto);

    Task<ApiResponse<IEnumerable<GoalsDetailDto>>> ListGoals();

    Task<ApiResponse<GoalsDetailDto>> ListGoalById(Guid id);

    Task<ApiResponse<string>> UpdateGoal(Guid id, UpdateGoalDto dto);

    Task<ApiResponse<string>> DeleteGoal(Guid id);
}
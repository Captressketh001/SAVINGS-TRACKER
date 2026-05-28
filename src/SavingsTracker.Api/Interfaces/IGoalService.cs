using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;


namespace SavingsTracker.Api.Interfaces;

public interface IGoalService
{
    Task<ApiResponse<string>> CreateGoal(CreateGoalDto dto);
}
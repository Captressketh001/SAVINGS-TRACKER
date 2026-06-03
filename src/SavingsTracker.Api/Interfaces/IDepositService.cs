using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;

namespace SavingsTracker.Api.Interfaces;

public interface IDepositService
{
    Task<ApiResponse<string>> AddDepositToGoal(Guid id, DepositDto dto);
    Task<ApiResponse<IEnumerable<DepositDetailDto>>> ListGoalDeposit(Guid id);
}
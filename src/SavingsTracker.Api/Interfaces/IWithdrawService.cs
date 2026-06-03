using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;

namespace SavingsTracker.Api.Interfaces;

public interface IWithdrawService
{
    Task<ApiResponse<string>> WithdrawFromGoal(Guid id, WithdrawDto dto);
    Task<ApiResponse<IEnumerable<DepositDetailDto>>> ListGoalWithdrawal(Guid id);
}
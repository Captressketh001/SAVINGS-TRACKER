using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class WithdrawService: IWithdrawService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WithdrawService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
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

    public async Task<ApiResponse<string>> WithdrawFromGoal(Guid id, WithdrawDto dto)
    {
        var userId = GetLoggedInUserId();
        var goal = await _unitOfWork.Goals.GetGoalWithDetailsAsync(id);

        if (goal is null)
        {
            return new ApiResponse<string>(
                ResponseMsg: "This goal does not exist. Create a goal to withdraw from",
                ResponseDetails: null,
                ResponseCode: 404
            );
        }
        if (goal.CreatedBy != userId)
        {
            return new ApiResponse<string>(
                ResponseMsg: "You are not authorized to withdraw from this goal",
                ResponseDetails: null,
                ResponseCode: 403
            );
        }

        var currentAmount = goal.Deposits.Sum(d => d.Amount) - goal.Withdrawals.Sum(d => d.Amount);

        if (currentAmount < dto.Amount)
        {
            return new ApiResponse<string>(
                ResponseMsg: "Insufficient Balance",
                ResponseDetails: null,
                ResponseCode: 400
            );
        }

        var withdrawal = new Withdrawal
        {
            Amount = dto.Amount,
            Note = dto.Note,
            GoalId = goal.Id
        };
        await _unitOfWork.Withdrawals.AddAsync(withdrawal);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<string>
        (
            ResponseMsg: "Withdrawal Successful!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<IEnumerable<DepositDetailDto>>> ListGoalWithdrawal(Guid id)
    {
        var userId = GetLoggedInUserId();

        var goal = await _unitOfWork.Goals.ExistsForUserAsync(id, userId);

        if (!goal)
            return new ApiResponse<IEnumerable<DepositDetailDto>>(
                ResponseMsg: "Goal not found",
                ResponseDetails: Enumerable.Empty<DepositDetailDto>(),
                ResponseCode: 404
            );
        var withdrawals = await _unitOfWork.Withdrawals.FindAsync(d => d.GoalId == id);

        if (!withdrawals.Any())
        {
            return new ApiResponse<IEnumerable<DepositDetailDto>>(
                ResponseMsg: "No withdrawal found for this goal",
                ResponseDetails: Enumerable.Empty<DepositDetailDto>(),
                ResponseCode: 200
            );
        }
        var withdrawalDtos = withdrawals.Select(d => new DepositDetailDto(
            d.Id,
            d.GoalId,
            d.Amount,
            d.Note,
            d.Date
        ));

        return new ApiResponse<IEnumerable<DepositDetailDto>>(
            ResponseMsg: "Withdrawal Retrieved Successfully!",
            ResponseDetails: withdrawalDtos,
            ResponseCode: 200
        );
    }
}
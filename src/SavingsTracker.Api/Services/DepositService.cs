using System.Security.Claims;
using SavingsTracker.Api.Dtos;
using SavingsTracker.Api.DTOs;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Models;

namespace SavingsTracker.Api.Services;

public class DepositService : IDepositService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepositService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
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

    public async Task<ApiResponse<string>> AddDepositToGoal(Guid id, DepositDto dto)
    {
        var userId = GetLoggedInUserId();
        var goal = await _unitOfWork.Goals.GetByIdAsync(id);

        if (goal is null)
        {
            return new ApiResponse<string>(
                ResponseMsg: "This goal does not exist. Create a goal to add a deposit",
                ResponseDetails: null,
                ResponseCode: 404
            );
        }
        if (goal.CreatedBy != userId)
        {
            return new ApiResponse<string>(
                ResponseMsg: "You are not authorized to deposit into this goal",
                ResponseDetails: null,
                ResponseCode: 403
            );
        }
        var deposit = new Deposit
        {
            Amount = dto.Amount,
            Note = dto.Note,
            GoalId = goal.Id
        };
        await _unitOfWork.Deposits.AddAsync(deposit);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<string>
        (
            ResponseMsg: "Goal Funded successfully!",
            ResponseDetails: null,
            ResponseCode: 200
        );
    }

    public async Task<ApiResponse<IEnumerable<DepositDetailDto>>> ListGoalDeposit(Guid id)
    {
        var userId = GetLoggedInUserId();

        var goal = await _unitOfWork.Goals
            .ExistsForUserAsync(id, userId);

        if (!goal)
            return new ApiResponse<IEnumerable<DepositDetailDto>>(
                ResponseMsg: "Goal not found",
                ResponseDetails: Enumerable.Empty<DepositDetailDto>(),
                ResponseCode: 404
            );
       var deposits = await _unitOfWork.Deposits.FindAsync(d => d.GoalId == id);

        if (!deposits.Any())
        {
            return new ApiResponse<IEnumerable<DepositDetailDto>>(
                ResponseMsg: "No deposits found for this goal",
                ResponseDetails: Enumerable.Empty<DepositDetailDto>(),
                ResponseCode: 200
            );
        }
        var depositDtos = deposits.Select(d => new DepositDetailDto(
            d.Id,
            d.GoalId,
            d.Amount,
            d.Note,
            d.Date
        ));

        return new ApiResponse<IEnumerable<DepositDetailDto>>(
            ResponseMsg: "Deposits Retrieved Successfully!",
            ResponseDetails: depositDtos,
            ResponseCode: 200
        );
    }
}
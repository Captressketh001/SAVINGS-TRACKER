namespace SavingsTracker.Api.DTOs;

public record ApiResponse<T>(
    string ResponseMsg,
    T? ResponseDetails,
    int ResponseCode
);
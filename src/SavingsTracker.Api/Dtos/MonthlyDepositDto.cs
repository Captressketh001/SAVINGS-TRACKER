namespace SavingsTracker.Api.Dtos;
public record MonthlyDepositDto
(
    int Year,  
    string Month,
    decimal TotalAmount
);
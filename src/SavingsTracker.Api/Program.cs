using SavingsTracker.Api.Endpoints;
using SavingsTracker.Api.Extensions;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Middleware;
using SavingsTracker.Api.Repositories;
using SavingsTracker.Api.Services;

using Scalar.AspNetCore;
// using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddPostgreDb();
builder.AddJwtAuthentication();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IWithdrawService, WithdrawService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.MapScalarApiReference(options =>
    {
        options.AddPreferredSecuritySchemes("Bearer")
            .AddHttpAuthentication("Bearer", bearer =>
            {
                bearer.Token = "";
            });
    });
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MigrateDatabase();
app.MapAuthEndpoints();
app.MapGoalEndpoints();
app.MapDepositEndpoints();
app.MapWithdrawEndpoints();
app.Run();



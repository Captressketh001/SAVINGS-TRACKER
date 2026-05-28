using SavingsTracker.Api.Endpoints;
using SavingsTracker.Api.Extensions;
using SavingsTracker.Api.Interfaces;
using SavingsTracker.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddPostgreDb();
builder.AddJwtAuthentication();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MigrateDatabase();
app.Run();



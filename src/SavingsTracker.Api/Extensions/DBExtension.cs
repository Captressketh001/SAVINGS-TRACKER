using Microsoft.EntityFrameworkCore;
using SavingsTracker.Api.Data;

namespace SavingsTracker.Api.Extensions;

public static class DatabaseExtension
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<SavingsStoreContext>();

        context.Database.Migrate();
    }

    public static void AddPostgreDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' was not found.");
        }

        builder.Services.AddDbContext<SavingsStoreContext>(options =>
            options.UseNpgsql(
                connString,
                o => o.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery)));
    }
}
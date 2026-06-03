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
        DotNetEnv.Env.Load();

        var connString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                         $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                         $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                         $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

        builder.Services.AddDbContext<SavingsStoreContext>(options =>
            options.UseNpgsql(connString, o => 
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
    }
}
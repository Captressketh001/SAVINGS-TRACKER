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

        if (connString.StartsWith("postgres://") ||
            connString.StartsWith("postgresql://"))
        {
            var uri = new Uri(connString);

            var userInfo = uri.UserInfo.Split(':', 2);

            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = Uri.UnescapeDataString(userInfo[1]);

            var database = uri.AbsolutePath.TrimStart('/');

            connString =
                $"Host={uri.Host};" +
                $"Port={uri.Port};" +
                $"Database={database};" +
                $"Username={username};" +
                $"Password={password}";
        }

        builder.Services.AddDbContext<SavingsStoreContext>(options =>
            options.UseNpgsql(
                connString,
                o => o.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery)));
    }
}
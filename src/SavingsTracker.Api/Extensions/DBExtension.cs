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

            if (userInfo.Length != 2)
            {
                throw new InvalidOperationException(
                    "Invalid PostgreSQL connection URL.");
            }

            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = Uri.UnescapeDataString(userInfo[1]);
            var database = uri.AbsolutePath.TrimStart('/');

            var port = uri.IsDefaultPort ? 5432 : uri.Port;

            connString =
                $"Host={uri.Host};" +
                $"Port={port};" +
                $"Database={database};" +
                $"Username={username};" +
                $"Password={password};";

            if (!connString.Contains("Ssl Mode=", StringComparison.OrdinalIgnoreCase))
            {
                connString += "Ssl Mode=Require;";
            }
        }

        builder.Services.AddDbContext<SavingsStoreContext>(options =>
            options.UseNpgsql(
                connString,
                o => o.UseQuerySplittingBehavior(
                    QuerySplittingBehavior.SplitQuery)));
    }
}
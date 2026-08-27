using CivicFix.Application.Common.Interfaces;
using CivicFix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CivicFixDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                var sqlServerConn = connectionString ?? "Server=localhost,1433;Database=CivicFixDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
                options.UseSqlServer(sqlServerConn, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
            }
            else if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(connectionString ?? "CivicFixInMemoryDb");
            }
            else
            {
                var sqliteConn = connectionString ?? "Data Source=civicfix.db";
                options.UseSqlite(sqliteConn);
            }
        });

        services.AddScoped<IApplicationDbContext>(providerService => providerService.GetRequiredService<CivicFixDbContext>());

        return services;
    }
}

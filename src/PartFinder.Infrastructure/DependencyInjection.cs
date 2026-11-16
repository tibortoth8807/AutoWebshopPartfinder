using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PartFinder.Infrastructure.Nhtsa;
using PartFinder.Infrastructure.Persistence;

namespace PartFinder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string nhtsaBaseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

        services.AddHttpClient<NhtsaVinClient>(client =>
            {
                client.BaseAddress = new Uri(nhtsaBaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PartFinder-demo/1.0");
            })
            .AddStandardResilienceHandler(); // retry, circuit breaker, timeouts

        return services;
    }

    /// <summary>Applies migrations if any exist (otherwise creates the schema), then seeds the demo catalog.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(ct);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(ct);
        }

        await CatalogSeeder.SeedAsync(db, ct);
    }
}

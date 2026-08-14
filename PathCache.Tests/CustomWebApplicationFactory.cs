using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PathCache.Api.Data;

namespace PathCache.Tests;

/// <summary>
/// Hosts the API in-process against a private in-memory database. Each factory
/// instance gets its own database name, so test classes never share state.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Non-Development environment so Program.cs skips the dev seeder.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PathCacheDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Program.cs's AddDbContext(...UseNpgsql...) eagerly registers Npgsql's
            // relational services into this same collection under EF Core's own
            // interface types, so they aren't identifiable by assembly and removing
            // them piecemeal is unreliable. Instead give the in-memory provider its
            // own isolated internal service provider so EF never sees both providers
            // registered together.
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<PathCacheDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });
        });
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Persistence.Context;

namespace TmsApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Define a dedicated connection string for your test database
    private const string TestConnectionString = 
        "Host=localhost;Database=tms_test_db;Username=postgres;Password=samidan@1010";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Supply required test configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Secret"] = "ThisIsASecretKeyForTestingPurposesOnly123456!",
                ["Jwt:Issuer"] = "TmsTestIssuer",
                ["Jwt:Audience"] = "TmsTestAudience",
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString
            });
        });

        // 2. Remove production DbContext and register PostgreSQL provider
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TmsDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<TmsDbContext>();

            services.AddDbContext<TmsDbContext>(options =>
            {
                options.UseNpgsql(TestConnectionString);
            });
        });
    }
}
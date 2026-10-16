using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VehicleRegistry.Api.Data;
using VehicleRegistry.Core.Domain;

namespace VehicleRegistry.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "VehicleRegistry_IntegrationTests";
    private readonly string _connectionString;

    public ApiFactory()
    {
        // Reuse the developer's connection (user secrets or environment variable),
        // but point it at a separate test database.
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var baseConnectionString = config.GetConnectionString("VehicleRegistry")
            ?? throw new InvalidOperationException(
                "No connection string found. Set the 'ConnectionStrings:VehicleRegistry' user secret " +
                "on the Api project, or the ConnectionStrings__VehicleRegistry environment variable.");

        _connectionString = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = TestDatabaseName
        }.ConnectionString;

        // Start every run from scratch; the app's startup migration recreates the database.
        using var db = CreateDbContext();
        db.Database.EnsureDeleted();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Replace the app's database registration with one pointing at the test database.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(_connectionString));
        });
    }

    /// <summary>Clears vehicles and restores the three default categories.</summary>
    public async Task ResetDatabaseAsync()
    {
        _ = Server; // ensures the app has started, which applies migrations to the test database

        await using var db = CreateDbContext();
        await db.Vehicles.ExecuteDeleteAsync();
        await db.WeightCategories.ExecuteDeleteAsync();

        db.WeightCategories.AddRange(
            new WeightCategory { Name = "Light", MinWeightKg = 0m, IconKey = "bike" },
            new WeightCategory { Name = "Medium", MinWeightKg = 500m, IconKey = "car" },
            new WeightCategory { Name = "Heavy", MinWeightKg = 2500m, IconKey = "truck" });
        await db.SaveChangesAsync();
    }

    private AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_connectionString).Options);
}
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using FinanceBackend.Data;
using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Users;
using FinanceBackend.Core;
using FinanceBackend.Models;

namespace FinanceBackend.Tests;

/// <summary>
/// Base integration test factory that wires up the app with an
/// in-memory SQLite database instead of real PostgreSQL.
/// </summary>
public class FinanceApiFactory : WebApplicationFactory<Program>
{
    // Known stable secret — injected into both JWT generation and validation in tests
    public const string TestJwtSecret = "test-secret-key-for-integration-tests-32chars!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override JWT config so the test server validates tokens signed with TestJwtSecret
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"]     = TestJwtSecret,
                ["Jwt:Issuer"]        = "FinanceBackend",
                ["Jwt:Audience"]      = "FinanceBackendUsers",
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real EF registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Create a stable db name for this execution so the seed maps to the API calls
            var dbName = "TestDb_" + Guid.NewGuid();

            // Replace with in-memory database
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User
        {
            Id             = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Email          = "admin@test.local",
            FullName       = "Test Admin",
            HashedPassword = PasswordService.Hash("Admin1234!"),
            Role           = UserRole.Admin,
            IsActive       = true
        };
        var analyst = new User
        {
            Id             = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Email          = "analyst@test.local",
            FullName       = "Test Analyst",
            HashedPassword = PasswordService.Hash("Analyst1234!"),
            Role           = UserRole.Analyst,
            IsActive       = true
        };
        var viewer = new User
        {
            Id             = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Email          = "viewer@test.local",
            FullName       = "Test Viewer",
            HashedPassword = PasswordService.Hash("Viewer1234!"),
            Role           = UserRole.Viewer,
            IsActive       = true
        };

        db.Users.AddRange(admin, analyst, viewer);

        db.Transactions.AddRange(
            new Transaction
            {
                Id              = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CreatedByUserId = admin.Id,
                Amount          = 5000m,
                Type            = TransactionType.Income,
                Category        = "Salary",
                Date            = new DateOnly(2025, 1, 1),
                Notes           = "January salary"
            },
            new Transaction
            {
                Id              = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                CreatedByUserId = admin.Id,
                Amount          = 1200m,
                Type            = TransactionType.Expense,
                Category        = "Rent",
                Date            = new DateOnly(2025, 1, 2)
            }
        );

        db.SaveChanges();
    }
}

public static class TestJsonHelper
{
    public static readonly System.Text.Json.JsonSerializerOptions Options = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static Task<T?> ReadJsonAsync<T>(this HttpContent content) =>
        content.ReadFromJsonAsync<T>(Options);
}

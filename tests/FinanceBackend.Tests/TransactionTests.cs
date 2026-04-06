using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Transactions;
using FinanceBackend.DTOs.Dashboard;

namespace FinanceBackend.Tests;

public class TransactionTests : IClassFixture<FinanceApiFactory>
{
    private readonly FinanceApiFactory _factory;

    public TransactionTests(FinanceApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthenticatedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var resp   = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var body   = await resp.Content.ReadJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    [Fact]
    public async Task GetTransactions_AsAdmin_ReturnsAllRecords()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.GetAsync("/api/v1/transactions");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<PagedResult<TransactionResponse>>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactions_AsViewer_ReturnsOnlyOwnRecords()
    {
        // Viewer user has no transactions in seed data
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");

        var resp = await client.GetAsync("/api/v1/transactions");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<PagedResult<TransactionResponse>>();
        body!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTransaction_AsAdmin_Returns201()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            amount   = 999.99,
            type     = "Income",
            category = "Freelance",
            date     = "2025-03-01",
            notes    = "Test transaction"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await resp.Content.ReadJsonAsync<TransactionResponse>();
        body!.Amount.Should().Be(999.99m);
        body.Category.Should().Be("Freelance");
    }

    [Fact]
    public async Task CreateTransaction_AsViewer_Returns403()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");

        var resp = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            amount   = 100,
            type     = "Income",
            category = "Test",
            date     = "2025-01-01"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTransaction_WithFutureDate_Returns400()
    {
        var client     = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");
        var futureDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd");

        var resp = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            amount   = 100,
            type     = "Income",
            category = "Test",
            date     = futureDate
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_Returns400()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            amount   = -50,
            type     = "Income",
            category = "Test",
            date     = "2025-01-01"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTransaction_AsAdmin_Returns204()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        // Create a transaction to delete
        var create = await client.PostAsJsonAsync("/api/v1/transactions", new
        {
            amount   = 50,
            type     = "Expense",
            category = "Test",
            date     = "2025-02-01"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var tx = await create.Content.ReadJsonAsync<TransactionResponse>();

        var del = await client.DeleteAsync($"/api/v1/transactions/{tx!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Confirm soft-deleted (not visible anymore)
        var get = await client.GetAsync($"/api/v1/transactions/{tx.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

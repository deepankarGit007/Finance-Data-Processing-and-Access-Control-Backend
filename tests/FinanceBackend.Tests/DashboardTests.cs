using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;

namespace FinanceBackend.Tests;

public class DashboardTests : IClassFixture<FinanceApiFactory>
{
    private readonly FinanceApiFactory _factory;

    public DashboardTests(FinanceApiFactory factory) => _factory = factory;

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
    public async Task Summary_AsViewer_Returns200WithCorrectTotals()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");

        var resp = await client.GetAsync("/api/v1/dashboard/summary");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<SummaryResponse>();
        body.Should().NotBeNull();
        // Seed: 5000 income, 1200 expense
        body!.TotalIncome.Should().Be(5000m);
        body.TotalExpenses.Should().Be(1200m);
        body.NetBalance.Should().Be(3800m);
    }

    [Fact]
    public async Task ByCategory_AsAnalyst_Returns200()
    {
        var client = await AuthenticatedClientAsync("analyst@test.local", "Analyst1234!");

        var resp = await client.GetAsync("/api/v1/dashboard/by-category");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<List<CategoryTotalResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(c => c.Category == "Salary");
    }

    [Fact]
    public async Task ByCategory_AsViewer_Returns403()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");

        var resp = await client.GetAsync("/api/v1/dashboard/by-category");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Trends_AsAdmin_Returns200WithData()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.GetAsync("/api/v1/dashboard/trends?months=6");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<List<MonthlyTrendResponse>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Recent_AsAdmin_ReturnsUpTo10Records()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.GetAsync("/api/v1/dashboard/recent?count=10");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadJsonAsync<List<TransactionResponse>>();
        body.Should().NotBeNull();
        body!.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public async Task Dashboard_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient(); // no auth
        var resp = await client.GetAsync("/api/v1/dashboard/summary");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

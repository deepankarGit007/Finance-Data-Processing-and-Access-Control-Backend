using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using FinanceBackend.DTOs.Auth;
using FinanceBackend.DTOs.Users;

namespace FinanceBackend.Tests;

public class UserTests : IClassFixture<FinanceApiFactory>
{
    private readonly FinanceApiFactory _factory;

    public UserTests(FinanceApiFactory factory)
    {
        _factory = factory;
    }

    /// <summary>Creates a fresh, isolated HttpClient with a valid JWT token set.</summary>
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
    public async Task GetAllUsers_AsAdmin_Returns200()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.GetAsync("/api/v1/users");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await resp.Content.ReadJsonAsync<List<UserResponse>>();
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllUsers_AsViewer_Returns403()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");

        var resp = await client.GetAsync("/api/v1/users");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_AsAdmin_Returns201()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.PostAsJsonAsync("/api/v1/users", new
        {
            email    = "newuser@test.local",
            fullName = "New Test User",
            password = "NewPass1234!",
            role     = "Viewer"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await resp.Content.ReadJsonAsync<UserResponse>();
        user!.Email.Should().Be("newuser@test.local");
        user.Role.Should().Be(Core.UserRole.Viewer);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Returns409()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        // admin@test.local already exists in seed data
        var resp = await client.PostAsJsonAsync("/api/v1/users", new
        {
            email    = "admin@test.local",
            fullName = "Dup User",
            password = "Admin1234!",
            role     = "Viewer"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUser_WeakPassword_Returns400()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        var resp = await client.PostAsJsonAsync("/api/v1/users", new
        {
            email    = "weak@test.local",
            fullName = "Weak User",
            password = "weak",   // too short, no uppercase, no digit
            role     = "Viewer"
        });

        // FluentValidation returns 400 for validation failures
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserById_AsSelf_Returns200()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");
        var viewerId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var resp = await client.GetAsync($"/api/v1/users/{viewerId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserById_AsViewerForAnotherUser_Returns403()
    {
        var client = await AuthenticatedClientAsync("viewer@test.local", "Viewer1234!");
        var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var resp = await client.GetAsync($"/api/v1/users/{adminId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateUser_AsAdmin_Returns204()
    {
        var client = await AuthenticatedClientAsync("admin@test.local", "Admin1234!");

        // Create a throwaway user first
        var create = await client.PostAsJsonAsync("/api/v1/users", new
        {
            email    = "todelete@test.local",
            fullName = "Delete Me",
            password = "Delete1234!",
            role     = "Viewer"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await create.Content.ReadJsonAsync<UserResponse>();

        var del = await client.DeleteAsync($"/api/v1/users/{user!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

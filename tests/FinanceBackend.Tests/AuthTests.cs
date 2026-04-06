using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FinanceBackend.DTOs.Auth;

namespace FinanceBackend.Tests;

public class AuthTests : IClassFixture<FinanceApiFactory>
{
    private readonly FinanceApiFactory _factory;

    public AuthTests(FinanceApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var client   = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email    = "admin@test.local",
            password = "Admin1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadJsonAsync<TokenResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.TokenType.Should().Be("Bearer");
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client   = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email    = "admin@test.local",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidEmail_Returns400()
    {
        var client   = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email    = "not-an-email",
            password = "Admin1234!"
        });

        // FluentValidation returns 400 Bad Request for validation failures
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var client = _factory.CreateClient();

        // Login first
        var loginResp = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email    = "analyst@test.local",
            password = "Analyst1234!"
        });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = (await loginResp.Content.ReadFromJsonAsync<TokenResponse>())!.AccessToken;

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var meResp = await client.GetAsync("/api/v1/auth/me");
        meResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await meResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        body.Should().ContainKey("email");
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

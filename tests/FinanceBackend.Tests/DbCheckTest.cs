using FinanceBackend.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceBackend.Tests;

public class DbCheckTest : IClassFixture<FinanceApiFactory>
{
    private readonly FinanceApiFactory _factory;

    public DbCheckTest(FinanceApiFactory factory) => _factory = factory;

    [Fact]
    public void CheckIfDbHasUsers()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Count().Should().Be(3);
    }
}

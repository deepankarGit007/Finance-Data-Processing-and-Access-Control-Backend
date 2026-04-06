using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceBackend.Core;
using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>
    /// Returns total income, expenses, and net balance.
    /// Available to all authenticated roles (Viewer, Analyst, Admin).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboard.GetSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Returns totals grouped by category and type.
    /// Analyst and Admin only.
    /// </summary>
    [HttpGet("by-category")]
    [Authorize(Roles = "Analyst,Admin")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTotalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory()
    {
        var result = await _dashboard.GetByCategoryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns monthly income/expense/net trends.
    /// Analyst and Admin only.
    /// </summary>
    [HttpGet("trends")]
    [Authorize(Roles = "Analyst,Admin")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyTrendResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrends([FromQuery] int months = 6)
    {
        var result = await _dashboard.GetMonthlyTrendsAsync(months);
        return Ok(result);
    }

    /// <summary>
    /// Returns the N most recent transactions.
    /// Available to all authenticated roles.
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
    {
        var result = await _dashboard.GetRecentAsync(count);
        return Ok(result);
    }
}

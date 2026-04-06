using Microsoft.EntityFrameworkCore;
using FinanceBackend.Core;
using FinanceBackend.Data;
using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<SummaryResponse> GetSummaryAsync()
    {
        var totals = await _db.Transactions
            .GroupBy(t => t.Type)
            .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount), Count = g.Count() })
            .ToListAsync();

        var income   = totals.FirstOrDefault(t => t.Type == TransactionType.Income);
        var expense  = totals.FirstOrDefault(t => t.Type == TransactionType.Expense);
        var incAmt   = income?.Total   ?? 0m;
        var expAmt   = expense?.Total  ?? 0m;
        var totalRec = (income?.Count  ?? 0) + (expense?.Count ?? 0);

        return new SummaryResponse
        {
            TotalIncome   = incAmt,
            TotalExpenses = expAmt,
            NetBalance    = incAmt - expAmt,
            TotalRecords  = totalRec
        };
    }

    public async Task<IEnumerable<CategoryTotalResponse>> GetByCategoryAsync()
    {
        var results = await _db.Transactions
            .GroupBy(t => new { t.Category, t.Type })
            .Select(g => new CategoryTotalResponse
            {
                Category = g.Key.Category,
                Type     = g.Key.Type.ToString(),
                Total    = g.Sum(t => t.Amount),
                Count    = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        return results;
    }

    public async Task<IEnumerable<MonthlyTrendResponse>> GetMonthlyTrendsAsync(int months)
    {
        months = Math.Clamp(months, 1, 24);
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months + 1));
        cutoff = new DateOnly(cutoff.Year, cutoff.Month, 1); // start of month

        var raw = await _db.Transactions
            .Where(t => t.Date >= cutoff)
            .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Type })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Type,
                Total = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        // Pivot into monthly trend rows
        var trend = raw
            .GroupBy(r => new { r.Year, r.Month })
            .Select(g => new MonthlyTrendResponse
            {
                Year    = g.Key.Year,
                Month   = g.Key.Month,
                Income  = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Total),
                Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Total),
                Net     = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Total)
                        - g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Total)
            })
            .OrderBy(t => t.Year).ThenBy(t => t.Month)
            .ToList();

        return trend;
    }

    public async Task<IEnumerable<TransactionResponse>> GetRecentAsync(int count)
    {
        count = Math.Clamp(count, 1, 50);

        var txs = await _db.Transactions
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();

        return txs.Select(TransactionResponse.FromModel);
    }
}

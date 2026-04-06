using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;

namespace FinanceBackend.Services.Interfaces;

public interface IDashboardService
{
    Task<SummaryResponse> GetSummaryAsync();
    Task<IEnumerable<CategoryTotalResponse>> GetByCategoryAsync();
    Task<IEnumerable<MonthlyTrendResponse>> GetMonthlyTrendsAsync(int months);
    Task<IEnumerable<TransactionResponse>> GetRecentAsync(int count);
}

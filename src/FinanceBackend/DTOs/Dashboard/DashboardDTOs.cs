namespace FinanceBackend.DTOs.Dashboard;

public class SummaryResponse
{
    public decimal TotalIncome   { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance    { get; set; }
    public int     TotalRecords  { get; set; }
}

public class CategoryTotalResponse
{
    public string  Category { get; set; } = string.Empty;
    public string  Type     { get; set; } = string.Empty;
    public decimal Total    { get; set; }
    public int     Count    { get; set; }
}

public class MonthlyTrendResponse
{
    public int     Year    { get; set; }
    public int     Month   { get; set; }
    public decimal Income  { get; set; }
    public decimal Expense { get; set; }
    public decimal Net     { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items      { get; set; } = Enumerable.Empty<T>();
    public int            TotalCount { get; set; }
    public int            Page       { get; set; }
    public int            PageSize   { get; set; }
    public int            TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

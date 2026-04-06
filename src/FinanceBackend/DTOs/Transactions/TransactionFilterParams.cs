using FinanceBackend.Core;

namespace FinanceBackend.DTOs.Transactions;

/// <summary>Query parameters for filtering/paginating transactions.</summary>
public class TransactionFilterParams
{
    public TransactionType? Type       { get; set; }
    public string?          Category   { get; set; }
    public DateOnly?        StartDate  { get; set; }
    public DateOnly?        EndDate    { get; set; }

    /// <summary>1-based page number (default 1)</summary>
    public int Page     { get; set; } = 1;

    /// <summary>Records per page (1–100, default 20)</summary>
    public int PageSize { get; set; } = 20;
}

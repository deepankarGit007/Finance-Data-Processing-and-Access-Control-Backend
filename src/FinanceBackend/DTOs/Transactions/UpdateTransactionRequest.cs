using System.ComponentModel.DataAnnotations;
using FinanceBackend.Core;

namespace FinanceBackend.DTOs.Transactions;

public class UpdateTransactionRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    public TransactionType? Type { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    public DateOnly? Date { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

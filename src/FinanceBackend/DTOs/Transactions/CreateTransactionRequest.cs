using System.ComponentModel.DataAnnotations;
using FinanceBackend.Core;

namespace FinanceBackend.DTOs.Transactions;

public class CreateTransactionRequest
{
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required, MaxLength(128)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

using FinanceBackend.Core;
using FinanceBackend.Models;

namespace FinanceBackend.DTOs.Transactions;

public class TransactionResponse
{
    public Guid            Id              { get; set; }
    public decimal         Amount          { get; set; }
    public TransactionType Type            { get; set; }
    public string          Category        { get; set; } = string.Empty;
    public DateOnly        Date            { get; set; }
    public string?         Notes           { get; set; }
    public Guid            CreatedByUserId { get; set; }
    public string          CreatedByName   { get; set; } = string.Empty;
    public DateTime        CreatedAt       { get; set; }
    public DateTime        UpdatedAt       { get; set; }

    public static TransactionResponse FromModel(Transaction t) => new()
    {
        Id              = t.Id,
        Amount          = t.Amount,
        Type            = t.Type,
        Category        = t.Category,
        Date            = t.Date,
        Notes           = t.Notes,
        CreatedByUserId = t.CreatedByUserId,
        CreatedByName   = t.CreatedBy?.FullName ?? string.Empty,
        CreatedAt       = t.CreatedAt,
        UpdatedAt       = t.UpdatedAt
    };
}

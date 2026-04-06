using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinanceBackend.Core;

namespace FinanceBackend.Models;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CreatedByUserId { get; set; }

    [Required]
    [Column(TypeName = "numeric(15,2)")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    [Required, MaxLength(128)]
    public string Category { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Soft-delete flag. Records are never physically removed.</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? CreatedBy { get; set; }
}

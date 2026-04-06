using Microsoft.EntityFrameworkCore;
using FinanceBackend.Core;
using FinanceBackend.Data;
using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;
using FinanceBackend.Models;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db) => _db = db;

    public async Task<PagedResult<TransactionResponse>> GetAllAsync(
        TransactionFilterParams filters,
        Guid? requestingUserId,
        bool isAdminOrAnalyst)
    {
        // Clamp page size to 1–100
        filters.PageSize = Math.Clamp(filters.PageSize, 1, 100);
        filters.Page     = Math.Max(filters.Page, 1);

        var query = _db.Transactions
            .Include(t => t.CreatedBy)
            .AsQueryable();

        // Viewers see only their own transactions
        if (!isAdminOrAnalyst && requestingUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == requestingUserId.Value);

        // Filters
        if (filters.Type.HasValue)
            query = query.Where(t => t.Type == filters.Type.Value);

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            var catLower = filters.Category.ToLower();
            query = query.Where(t => t.Category.ToLower().Contains(catLower));
        }

        if (filters.StartDate.HasValue)
            query = query.Where(t => t.Date >= filters.StartDate.Value);

        if (filters.EndDate.HasValue)
            query = query.Where(t => t.Date <= filters.EndDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<TransactionResponse>
        {
            Items      = items.Select(TransactionResponse.FromModel),
            TotalCount = totalCount,
            Page       = filters.Page,
            PageSize   = filters.PageSize
        };
    }

    public async Task<TransactionResponse> GetByIdAsync(Guid id)
    {
        var tx = await _db.Transactions
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaction '{id}' not found.");

        return TransactionResponse.FromModel(tx);
    }

    public async Task<TransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        Guid createdByUserId)
    {
        var tx = new Transaction
        {
            Amount          = request.Amount,
            Type            = request.Type,
            Category        = request.Category.Trim(),
            Date            = request.Date,
            Notes           = request.Notes?.Trim(),
            CreatedByUserId = createdByUserId
        };

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(tx).Reference(t => t.CreatedBy).LoadAsync();
        return TransactionResponse.FromModel(tx);
    }

    public async Task<TransactionResponse> UpdateAsync(Guid id, UpdateTransactionRequest request)
    {
        var tx = await _db.Transactions
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaction '{id}' not found.");

        if (request.Amount.HasValue)   tx.Amount   = request.Amount.Value;
        if (request.Type.HasValue)     tx.Type     = request.Type.Value;
        if (request.Category is not null) tx.Category = request.Category.Trim();
        if (request.Date.HasValue)     tx.Date     = request.Date.Value;
        if (request.Notes is not null) tx.Notes    = request.Notes.Trim();

        tx.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return TransactionResponse.FromModel(tx);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var tx = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Transaction '{id}' not found.");

        tx.IsDeleted = true;
        tx.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

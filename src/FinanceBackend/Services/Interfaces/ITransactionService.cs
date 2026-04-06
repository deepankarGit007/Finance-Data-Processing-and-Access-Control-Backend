using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;

namespace FinanceBackend.Services.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionResponse>> GetAllAsync(TransactionFilterParams filters, Guid? requestingUserId, bool isAdmin);
    Task<TransactionResponse> GetByIdAsync(Guid id);
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, Guid createdByUserId);
    Task<TransactionResponse> UpdateAsync(Guid id, UpdateTransactionRequest request);
    Task SoftDeleteAsync(Guid id);
}

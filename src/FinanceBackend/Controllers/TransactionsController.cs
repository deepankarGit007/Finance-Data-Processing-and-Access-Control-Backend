using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceBackend.Core;
using FinanceBackend.DTOs.Dashboard;
using FinanceBackend.DTOs.Transactions;
using FinanceBackend.Services.Interfaces;

namespace FinanceBackend.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _txService;

    public TransactionsController(ITransactionService txService) => _txService = txService;

    /// <summary>
    /// List transactions with optional filters and pagination.
    /// Viewers see only their own records; Analysts and Admins see all.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TransactionFilterParams filters)
    {
        var callerId        = GetCallerId();
        var isAdminOrAnalyst = User.IsInRole(UserRole.Admin.ToString())
                            || User.IsInRole(UserRole.Analyst.ToString());

        var result = await _txService.GetAllAsync(filters, callerId, isAdminOrAnalyst);
        return Ok(result);
    }

    /// <summary>Get a single transaction by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tx = await _txService.GetByIdAsync(id);
        return Ok(tx);
    }

    /// <summary>Create a new transaction. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var callerId = GetCallerId();
        var tx       = await _txService.CreateAsync(request, callerId);
        return CreatedAtAction(nameof(GetById), new { id = tx.Id }, tx);
    }

    /// <summary>Update an existing transaction. Admin only.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        var tx = await _txService.UpdateAsync(id, request);
        return Ok(tx);
    }

    /// <summary>Soft-delete a transaction. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _txService.SoftDeleteAsync(id);
        return NoContent();
    }

    private Guid GetCallerId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Invalid token."));
}

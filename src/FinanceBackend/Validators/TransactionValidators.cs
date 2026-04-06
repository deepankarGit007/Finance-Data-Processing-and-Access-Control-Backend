using FluentValidation;
using FinanceBackend.DTOs.Transactions;

namespace FinanceBackend.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(1_000_000_000).WithMessage("Amount cannot exceed 1,000,000,000.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type must be 'Income' or 'Expense'.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(128)
            .Matches(@"^[a-zA-Z0-9\s\-&/]+$").WithMessage("Category contains invalid characters.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Transaction date cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Category)
            .MaximumLength(128)
            .When(x => x.Category is not null);

        RuleFor(x => x.Date)
            .Must(d => d!.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Transaction date cannot be in the future.")
            .When(x => x.Date.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);
    }
}

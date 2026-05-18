using MediatR;

namespace CashFlow.Transactions.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    decimal Amount,
    string Type,
    string? Description,
    DateTime? OccurredAt
) : IRequest<CreateTransactionResult>;

public record CreateTransactionResult(Guid Id, decimal Amount, string Type, DateTime OccurredAt);

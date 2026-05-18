using MediatR;

namespace CashFlow.Transactions.Application.Queries.GetTransactions;

public record GetTransactionsQuery(DateOnly? Date) : IRequest<IEnumerable<TransactionDto>>;

public record TransactionDto(Guid Id, decimal Amount, string Type, string? Description, DateTime OccurredAt);

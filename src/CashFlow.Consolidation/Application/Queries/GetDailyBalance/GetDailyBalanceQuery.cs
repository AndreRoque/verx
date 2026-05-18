using MediatR;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalance;

public record GetDailyBalanceQuery(DateOnly Date) : IRequest<DailyBalanceDto?>;

public record DailyBalanceDto(DateOnly Date, decimal TotalCredits, decimal TotalDebits, decimal Balance, int TransactionCount, DateTime LastUpdatedAt);

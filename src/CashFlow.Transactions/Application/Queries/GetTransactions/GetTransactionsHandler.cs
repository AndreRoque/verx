using CashFlow.Transactions.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Transactions.Application.Queries.GetTransactions;

public class GetTransactionsHandler(TransactionsDbContext db) : IRequestHandler<GetTransactionsQuery, IEnumerable<TransactionDto>>
{
    public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Transactions.AsNoTracking();

        if (request.Date.HasValue)
        {
            var start = request.Date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = start.AddDays(1);
            query = query.Where(t => t.OccurredAt >= start && t.OccurredAt < end);
        }

        return await query
            .OrderByDescending(t => t.OccurredAt)
            .Select(t => new TransactionDto(t.Id, t.Amount, t.Type.ToString().ToLower(), t.Description, t.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}

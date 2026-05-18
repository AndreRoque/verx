using CashFlow.Shared.Events;
using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Enums;
using CashFlow.Transactions.Infrastructure.Data;
using MassTransit;
using MediatR;

namespace CashFlow.Transactions.Application.Commands.CreateTransaction;

public class CreateTransactionHandler(
    TransactionsDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateTransactionHandler> logger) : IRequestHandler<CreateTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, ignoreCase: true, out var type))
            throw new ArgumentException($"Invalid transaction type: '{request.Type}'. Use 'debit' or 'credit'.");

        var transaction = Transaction.Create(request.Amount, type, request.Description, request.OccurredAt);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction {Id} persisted: {Type} of {Amount} on {OccurredAt}",
            transaction.Id, transaction.Type, transaction.Amount, transaction.OccurredAt);

        try
        {
            await publishEndpoint.Publish(new TransactionCreatedEvent(
                transaction.Id,
                transaction.Amount,
                transaction.Type.ToString().ToLower(),
                transaction.OccurredAt,
                transaction.Description
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            // Transaction is persisted but event was not delivered.
            // Consolidation will not be updated until this is resolved (see Outbox Pattern roadmap).
            logger.LogError(ex,
                "Failed to publish TransactionCreatedEvent for transaction {Id}. " +
                "Transaction is persisted but consolidation update was not triggered.",
                transaction.Id);
            throw;
        }

        return new CreateTransactionResult(transaction.Id, transaction.Amount, request.Type, transaction.OccurredAt);
    }
}

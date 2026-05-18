namespace CashFlow.Shared.Events;

public record TransactionCreatedEvent(
    Guid TransactionId,
    decimal Amount,
    string Type, // "debit" ou "credit"
    DateTime OccurredAt,
    string? Description
);

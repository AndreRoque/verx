using CashFlow.Transactions.Domain.Enums;

namespace CashFlow.Transactions.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Transaction() { }

    public static Transaction Create(decimal amount, TransactionType type, string? description, DateTime? occurredAt = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        return new Transaction
        {
            Amount = amount,
            Type = type,
            Description = description,
            OccurredAt = occurredAt?.ToUniversalTime() ?? DateTime.UtcNow
        };
    }
}

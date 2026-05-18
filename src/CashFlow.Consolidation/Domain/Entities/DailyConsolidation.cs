namespace CashFlow.Consolidation.Domain.Entities;

public class DailyConsolidation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateOnly Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance => TotalCredits - TotalDebits;
    public int TransactionCount { get; private set; }
    public DateTime LastUpdatedAt { get; private set; } = DateTime.UtcNow;

    private DailyConsolidation() { }

    public static DailyConsolidation Create(DateOnly date) => new() { Date = date };

    public void ApplyCredit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException($"Credit amount must be positive, got {amount}.", nameof(amount));

        TotalCredits += amount;
        TransactionCount++;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDebit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException($"Debit amount must be positive, got {amount}.", nameof(amount));

        TotalDebits += amount;
        TransactionCount++;
        LastUpdatedAt = DateTime.UtcNow;
    }
}

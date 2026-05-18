using CashFlow.Consolidation.Application.Consumers;
using CashFlow.Consolidation.Infrastructure.Data;
using CashFlow.Shared.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CashFlow.Consolidation.Tests.Application.Consumers;

public class TransactionCreatedConsumerTests
{
    private ConsolidationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ConsolidationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ConsolidationDbContext(options);
    }

    [Fact]
    public async Task Consume_CreditEvent_ShouldUpdateConsolidation()
    {
        var db = CreateInMemoryContext();
        var consumer = new TransactionCreatedConsumer(db, NullLogger<TransactionCreatedConsumer>.Instance);

        var @event = new TransactionCreatedEvent(Guid.NewGuid(), 100m, "credit", DateTime.UtcNow, "Test");
        var context = Mock.Of<ConsumeContext<TransactionCreatedEvent>>(c =>
            c.Message == @event && c.CancellationToken == CancellationToken.None);

        await consumer.Consume(context);

        var consolidation = db.DailyConsolidations.Single();
        consolidation.TotalCredits.Should().Be(100m);
        consolidation.TotalDebits.Should().Be(0m);
        consolidation.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task Consume_DebitEvent_ShouldUpdateConsolidation()
    {
        var db = CreateInMemoryContext();
        var consumer = new TransactionCreatedConsumer(db, NullLogger<TransactionCreatedConsumer>.Instance);

        var @event = new TransactionCreatedEvent(Guid.NewGuid(), 50m, "debit", DateTime.UtcNow, "Test");
        var context = Mock.Of<ConsumeContext<TransactionCreatedEvent>>(c =>
            c.Message == @event && c.CancellationToken == CancellationToken.None);

        await consumer.Consume(context);

        var consolidation = db.DailyConsolidations.Single();
        consolidation.TotalDebits.Should().Be(50m);
        consolidation.Balance.Should().Be(-50m);
    }

    [Fact]
    public async Task Consume_MultipleEvents_ShouldAccumulateOnSameDay()
    {
        var db = CreateInMemoryContext();
        var consumer = new TransactionCreatedConsumer(db, NullLogger<TransactionCreatedConsumer>.Instance);
        var today = DateTime.UtcNow;

        var event1 = new TransactionCreatedEvent(Guid.NewGuid(), 300m, "credit", today, null);
        var event2 = new TransactionCreatedEvent(Guid.NewGuid(), 100m, "debit", today, null);

        var ctx1 = Mock.Of<ConsumeContext<TransactionCreatedEvent>>(c => c.Message == event1 && c.CancellationToken == CancellationToken.None);
        var ctx2 = Mock.Of<ConsumeContext<TransactionCreatedEvent>>(c => c.Message == event2 && c.CancellationToken == CancellationToken.None);

        await consumer.Consume(ctx1);
        await consumer.Consume(ctx2);

        db.DailyConsolidations.Should().HaveCount(1);
        var consolidation = db.DailyConsolidations.Single();
        consolidation.TotalCredits.Should().Be(300m);
        consolidation.TotalDebits.Should().Be(100m);
        consolidation.Balance.Should().Be(200m);
        consolidation.TransactionCount.Should().Be(2);
    }
}

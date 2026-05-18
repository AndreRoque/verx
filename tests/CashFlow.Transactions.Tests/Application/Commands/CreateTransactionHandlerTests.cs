using CashFlow.Shared.Events;
using CashFlow.Transactions.Application.Commands.CreateTransaction;
using CashFlow.Transactions.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using FluentAssertions;

namespace CashFlow.Transactions.Tests.Application.Commands;

public class CreateTransactionHandlerTests
{
    private TransactionsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TransactionsDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCreditTransaction_ShouldPersistAndPublishEvent()
    {
        var db = CreateInMemoryContext();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new CreateTransactionHandler(db, publishEndpoint.Object, NullLogger<CreateTransactionHandler>.Instance);

        var command = new CreateTransactionCommand(100.00m, "credit", "Test credit", null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(100.00m);
        result.Type.Should().Be("credit");
        db.Transactions.Should().HaveCount(1);
        publishEndpoint.Verify(p => p.Publish(It.IsAny<TransactionCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDebitTransaction_ShouldPersistAndPublishEvent()
    {
        var db = CreateInMemoryContext();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new CreateTransactionHandler(db, publishEndpoint.Object, NullLogger<CreateTransactionHandler>.Instance);

        var command = new CreateTransactionCommand(50.00m, "debit", "Test debit", null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(50.00m);
        result.Type.Should().Be("debit");
        db.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_InvalidAmount_ShouldThrowArgumentException()
    {
        var db = CreateInMemoryContext();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new CreateTransactionHandler(db, publishEndpoint.Object, NullLogger<CreateTransactionHandler>.Instance);

        var command = new CreateTransactionCommand(-10m, "credit", null, null);
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidType_ShouldThrowArgumentException()
    {
        var db = CreateInMemoryContext();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new CreateTransactionHandler(db, publishEndpoint.Object, NullLogger<CreateTransactionHandler>.Instance);

        var command = new CreateTransactionCommand(100m, "invalid_type", null, null);
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }
}

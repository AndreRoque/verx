using CashFlow.Consolidation.Application.Queries.GetDailyBalance;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CashFlow.Consolidation.Tests.Application.Queries;

public class GetDailyBalanceHandlerTests
{
    private ConsolidationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ConsolidationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ConsolidationDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingConsolidation_ShouldReturnDto()
    {
        var db = CreateInMemoryContext();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((byte[]?)null);

        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var consolidation = DailyConsolidation.Create(date);
        consolidation.ApplyCredit(200m);
        consolidation.ApplyDebit(50m);
        db.DailyConsolidations.Add(consolidation);
        await db.SaveChangesAsync();

        var handler = new GetDailyBalanceHandler(db, cache.Object);
        var result = await handler.Handle(new GetDailyBalanceQuery(date), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCredits.Should().Be(200m);
        result.TotalDebits.Should().Be(50m);
        result.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task Handle_NonExistingDate_ShouldReturnNull()
    {
        var db = CreateInMemoryContext();
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((byte[]?)null);

        var handler = new GetDailyBalanceHandler(db, cache.Object);
        var result = await handler.Handle(new GetDailyBalanceQuery(DateOnly.FromDateTime(DateTime.UtcNow)), CancellationToken.None);

        result.Should().BeNull();
    }
}

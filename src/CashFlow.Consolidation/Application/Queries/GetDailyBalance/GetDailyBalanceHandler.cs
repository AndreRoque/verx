using CashFlow.Consolidation.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalance;

public class GetDailyBalanceHandler(
    ConsolidationDbContext db,
    IDistributedCache cache,
    ILogger<GetDailyBalanceHandler> logger) : IRequestHandler<GetDailyBalanceQuery, DailyBalanceDto?>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);

    public async Task<DailyBalanceDto?> Handle(GetDailyBalanceQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"consolidation:{request.Date:yyyy-MM-dd}";

        var cached = await TryGetFromCacheAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        logger.LogDebug("Cache miss for {CacheKey}, querying database", cacheKey);

        var consolidation = await db.DailyConsolidations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Date == request.Date, cancellationToken);

        if (consolidation is null)
            return null;

        var dto = new DailyBalanceDto(
            consolidation.Date,
            consolidation.TotalCredits,
            consolidation.TotalDebits,
            consolidation.Balance,
            consolidation.TransactionCount,
            consolidation.LastUpdatedAt);

        await TrySetCacheAsync(cacheKey, dto, cancellationToken);

        return dto;
    }

    private async Task<DailyBalanceDto?> TryGetFromCacheAsync(string key, CancellationToken ct)
    {
        try
        {
            var cached = await cache.GetStringAsync(key, ct);
            if (cached is null) return null;
            return JsonSerializer.Deserialize<DailyBalanceDto>(cached);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for key {CacheKey}, falling back to database", key);
            return null;
        }
    }

    private async Task TrySetCacheAsync(string key, DailyBalanceDto dto, CancellationToken ct)
    {
        try
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(dto),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache write failed for key {CacheKey}, result will not be cached", key);
        }
    }
}

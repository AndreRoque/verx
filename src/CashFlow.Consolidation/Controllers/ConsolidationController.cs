using CashFlow.Consolidation.Application.Queries.GetDailyBalance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Consolidation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConsolidationController(IMediator mediator) : ControllerBase
{
    [HttpGet("{date}")]
    [Authorize(Roles = "consolidation:read")]
    [ProducesResponseType(typeof(DailyBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDailyBalance(DateOnly date, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDailyBalanceQuery(date), ct);
        return result is null ? NotFound(new { error = $"No consolidation found for {date:yyyy-MM-dd}" }) : Ok(result);
    }
}

using CashFlow.Transactions.Application.Commands.CreateTransaction;
using CashFlow.Transactions.Application.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Transactions.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "transactions:write")]
    [ProducesResponseType(typeof(CreateTransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new CreateTransactionCommand(
                request.Amount, request.Type, request.Description, request.OccurredAt), ct);
            return CreatedAtAction(nameof(GetAll), new { }, result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Validation failed for CreateTransaction: {Error} | Amount={Amount} Type={Type}",
                ex.Message, request.Amount, request.Type);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "transactions:read")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTransactionsQuery(date), ct);
        return Ok(result);
    }
}

public record CreateTransactionRequest(decimal Amount, string Type, string? Description, DateTime? OccurredAt);

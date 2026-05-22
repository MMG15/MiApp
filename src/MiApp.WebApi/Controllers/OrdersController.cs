using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Features.Ordenes.Commands.CrearOrden;

namespace MiApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Crear una nueva orden (requiere JWT)</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearOrdenRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CrearOrdenCommand(userId, request.Items);
        var result = await _mediator.Send(command, ct);
        return Created(string.Empty, result);
    }
}

public record CrearOrdenRequest(List<OrderItemInput> Items);

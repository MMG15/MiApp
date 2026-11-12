using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Features.Ordenes.Commands.ConfirmarOrden;
using MiApp.Application.Features.Ordenes.Commands.CrearOrden;
using MiApp.Application.Features.Ordenes.Queries.GetAllOrdenes;
using MiApp.Application.Features.Ordenes.Queries.GetMisOrdenes;
using MiApp.Application.Features.Ordenes.Queries.GetOrdenById;
using MiApp.WebApi.Extensions;

namespace MiApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/orders
[Authorize]                   // todos los endpoints requieren JWT
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Crear una orden. Descuenta stock y queda en estado Pending.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearOrdenRequest request, CancellationToken ct)
    {
        var command = new CrearOrdenCommand(User.GetUserId(), request.Items);
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Confirmar (pagar) una orden: el e-commerce llama a PaymentService por HTTP.
    /// Aprobado → Paid. Rechazado → PaymentRejected (devuelve el stock).
    /// Si PaymentService no responde → 503 y la orden sigue Pending.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmarOrdenCommand(id, User.GetUserId()), ct);
        return Ok(result);
    }

    /// <summary>Ver una orden (el dueño o un Admin).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetOrdenByIdQuery(id, User.GetUserId(), User.IsInRole("Admin"));
        return Ok(await _mediator.Send(query, ct));
    }

    /// <summary>Las órdenes del usuario logueado.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMisOrdenesQuery(User.GetUserId()), ct));

    /// <summary>Todas las órdenes del sistema (solo Admin).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllOrdenesQuery(), ct));
}

/// <summary>Body de POST /api/orders. El usuario sale del token, no del body.</summary>
public record CrearOrdenRequest(List<OrderItemInput> Items);

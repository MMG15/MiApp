using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Features.Payments.Commands.ProcessPayment;
using PaymentService.Application.Features.Payments.Queries.GetAllPayments;
using PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/payments
[Authorize]                   // exige un JWT válido emitido por el e-commerce
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Procesar el pago de una orden. Responde 200 con status "Approved" o "Rejected".
    /// Regla: se aprueba si la moneda es ARS y el monto es menor a 100000.
    /// Es idempotente: si la orden ya tiene un pago, devuelve el mismo resultado.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var command = new ProcessPaymentCommand(request.OrderId, request.Amount, request.Currency ?? "ARS");
        return Ok(await _mediator.Send(command, ct));
    }

    /// <summary>Consultar el pago de una orden.</summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), ct));

    /// <summary>Listar todos los pagos procesados.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllPaymentsQuery(), ct));
}

/// <summary>Body de POST /api/payments/process: { "orderId": "...", "amount": 45000, "currency": "ARS" }</summary>
public record ProcessPaymentRequest(Guid OrderId, decimal Amount, string? Currency);

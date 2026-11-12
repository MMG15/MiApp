using MediatR;
using PaymentService.Application.Features.Payments.Common;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

/// <summary>Procesar el pago de una orden. Command: modifica estado (crea un Payment).</summary>
public record ProcessPaymentCommand(Guid OrderId, decimal Amount, string Currency) : IRequest<PaymentDto>;

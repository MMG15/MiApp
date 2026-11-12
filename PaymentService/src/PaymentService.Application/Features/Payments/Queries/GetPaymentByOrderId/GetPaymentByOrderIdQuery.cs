using MediatR;
using PaymentService.Application.Features.Payments.Common;

namespace PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;

/// <summary>Consultar el pago de una orden. Query: solo lee, no modifica nada.</summary>
public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentDto>;

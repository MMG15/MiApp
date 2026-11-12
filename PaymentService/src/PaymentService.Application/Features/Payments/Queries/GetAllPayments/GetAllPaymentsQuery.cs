using MediatR;
using PaymentService.Application.Features.Payments.Common;

namespace PaymentService.Application.Features.Payments.Queries.GetAllPayments;

/// <summary>Listar todos los pagos procesados (útil para verificar el flujo).</summary>
public record GetAllPaymentsQuery() : IRequest<IReadOnlyList<PaymentDto>>;

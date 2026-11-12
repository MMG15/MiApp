using PaymentService.Domain.Entities;

namespace PaymentService.Application.Features.Payments.Common;

/// <summary>
/// DTO de salida (contrato de respuesta). Es lo que recibe el e-commerce:
/// { paymentId, orderId, status, transactionId, amount, currency, reason, processedAt }
/// </summary>
public record PaymentDto(
    Guid PaymentId,
    Guid OrderId,
    string Status,
    string TransactionId,
    decimal Amount,
    string Currency,
    string? Reason,
    DateTime ProcessedAt)
{
    public static PaymentDto FromEntity(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Status.ToString(),
        payment.TransactionId,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Reason,
        payment.ProcessedAt);
}

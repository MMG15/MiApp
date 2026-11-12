namespace MiApp.Application.Contracts.Infrastructure;

/// <summary>
/// Puerto (interfaz) hacia el servicio de pagos.
/// Application solo sabe QUÉ necesita (procesar un pago); no sabe que del otro lado hay HTTP.
/// La implementación concreta (PaymentClient con HttpClient) vive en Infrastructure.
/// </summary>
public interface IPaymentClient
{
    /// <exception cref="Common.Exceptions.ServiceUnavailableException">
    /// Si el servicio de pagos no responde, tarda demasiado o devuelve un error.
    /// </exception>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken ct = default);
}

/// <summary>Lo que el e-commerce le pide al servicio de pagos.</summary>
public record PaymentRequest(Guid OrderId, decimal Amount, string Currency);

/// <summary>Lo que el e-commerce necesita saber de la respuesta.</summary>
public record PaymentResult(bool Approved, string TransactionId, string? Reason);

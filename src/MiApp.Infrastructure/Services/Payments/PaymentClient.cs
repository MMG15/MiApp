using System.Net.Http.Json;
using MiApp.Application.Common.Exceptions;
using MiApp.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MiApp.Infrastructure.Services.Payments;

/// <summary>
/// Adaptador HTTP: implementa el puerto IPaymentClient llamando a PaymentService.
/// Es un "typed client": el HttpClient lo crea y configura IHttpClientFactory
/// (BaseAddress, timeout, reintentos) en Program.cs; acá solo se usa.
/// </summary>
public class PaymentClient : IPaymentClient
{
    private const string ProcessPaymentPath = "api/payments/process";
    private const string UnavailableMessage =
        "El servicio de pagos no está disponible en este momento. " +
        "La orden sigue en estado Pending: podés volver a confirmarla más tarde.";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentClient> _logger;

    public PaymentClient(HttpClient httpClient, ILogger<PaymentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        var body = new ProcessPaymentHttpRequest(request.OrderId, request.Amount, request.Currency);

        HttpResponseMessage response;
        try
        {
            // System.Text.Json serializa el DTO a JSON (camelCase)
            response = await _httpClient.PostAsJsonAsync(ProcessPaymentPath, body, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // Servicio apagado (HttpRequestException), timeout (TaskCanceledException
            // o TimeoutRejectedException de Polly) después de agotar los reintentos.
            _logger.LogWarning(ex, "No se pudo contactar a PaymentService para la orden {OrderId}", request.OrderId);
            throw new ServiceUnavailableException(UnavailableMessage, ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PaymentService respondió {StatusCode} para la orden {OrderId}",
                    (int)response.StatusCode, request.OrderId);
                throw new ServiceUnavailableException(
                    $"El servicio de pagos respondió con un error ({(int)response.StatusCode}). {UnavailableMessage}");
            }

            var result = await response.Content.ReadFromJsonAsync<ProcessPaymentHttpResponse>(cancellationToken: ct)
                ?? throw new ServiceUnavailableException("El servicio de pagos devolvió una respuesta vacía.");

            var approved = string.Equals(result.Status, "Approved", StringComparison.OrdinalIgnoreCase);
            return new PaymentResult(approved, result.TransactionId, result.Reason);
        }
    }

    // Contrato HTTP (cómo viaja el JSON). Es propio del e-commerce: no comparte clases con PaymentService.
    private sealed record ProcessPaymentHttpRequest(Guid OrderId, decimal Amount, string Currency);

    private sealed record ProcessPaymentHttpResponse(
        Guid PaymentId,
        Guid OrderId,
        string Status,
        string TransactionId,
        string? Reason);
}

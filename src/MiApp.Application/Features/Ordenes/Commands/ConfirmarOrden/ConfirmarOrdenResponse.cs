namespace MiApp.Application.Features.Ordenes.Commands.ConfirmarOrden;

public record ConfirmarOrdenResponse(
    Guid OrderId,
    string Estado,
    decimal Total,
    string? TransactionId,
    string Mensaje);

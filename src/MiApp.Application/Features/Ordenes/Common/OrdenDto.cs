using MiApp.Domain.Entities;

namespace MiApp.Application.Features.Ordenes.Common;

/// <summary>DTO de salida: lo que la API muestra de una orden (nunca la entidad).</summary>
public record OrdenDto(
    Guid Id,
    Guid UserId,
    string Estado,
    decimal Total,
    string Moneda,
    DateTime CreadoEn,
    string? TransactionId,
    DateTime? PagoProcesadoEn,
    IReadOnlyList<OrdenItemDto> Items)
{
    public static OrdenDto FromEntity(Order order) => new(
        order.Id,
        order.UserId,
        order.Status.ToString(),
        order.Total.Amount,
        order.Total.Currency,
        order.CreatedAt,
        order.TransactionId,
        order.PaymentProcessedAt,
        order.Items
            .Select(i => new OrdenItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.Subtotal.Amount))
            .ToList());
}

public record OrdenItemDto(Guid ProductId, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

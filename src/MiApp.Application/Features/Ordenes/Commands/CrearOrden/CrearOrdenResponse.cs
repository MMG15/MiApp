namespace MiApp.Application.Features.Ordenes.Commands.CrearOrden;

public record CrearOrdenResponse(
    Guid Id,
    decimal Total,
    string Estado,
    DateTime CreadoEn);

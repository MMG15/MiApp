using MediatR;

namespace MiApp.Application.Features.Ordenes.Commands.ConfirmarOrden;

/// <summary>
/// Confirmar (pagar) una orden. El UserId sale del token JWT, no del body.
/// </summary>
public record ConfirmarOrdenCommand(Guid OrderId, Guid UserId) : IRequest<ConfirmarOrdenResponse>;

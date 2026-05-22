using MediatR;

namespace MiApp.Application.Features.Ordenes.Commands.CrearOrden;

public record OrderItemInput(Guid ProductId, int Quantity);

public record CrearOrdenCommand(
    Guid UserId,
    List<OrderItemInput> Items) : IRequest<CrearOrdenResponse>;

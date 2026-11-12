using MediatR;
using MiApp.Application.Features.Ordenes.Common;

namespace MiApp.Application.Features.Ordenes.Queries.GetOrdenById;

/// <summary>Ver una orden. El dueño o un Admin pueden verla.</summary>
public record GetOrdenByIdQuery(Guid OrderId, Guid UserId, bool EsAdmin) : IRequest<OrdenDto>;

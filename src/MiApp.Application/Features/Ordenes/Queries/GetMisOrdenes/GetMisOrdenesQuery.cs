using MediatR;
using MiApp.Application.Features.Ordenes.Common;

namespace MiApp.Application.Features.Ordenes.Queries.GetMisOrdenes;

/// <summary>Las órdenes del usuario logueado.</summary>
public record GetMisOrdenesQuery(Guid UserId) : IRequest<IReadOnlyList<OrdenDto>>;

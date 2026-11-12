using MediatR;
using MiApp.Application.Features.Ordenes.Common;

namespace MiApp.Application.Features.Ordenes.Queries.GetAllOrdenes;

/// <summary>Todas las órdenes del sistema (solo Admin, se controla en el controller).</summary>
public record GetAllOrdenesQuery() : IRequest<IReadOnlyList<OrdenDto>>;

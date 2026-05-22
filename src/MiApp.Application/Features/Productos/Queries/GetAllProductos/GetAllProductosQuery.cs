using MediatR;
using MiApp.Application.Features.Productos.Queries.GetProductoById;

namespace MiApp.Application.Features.Productos.Queries.GetAllProductos;

public record GetAllProductosQuery() : IRequest<IReadOnlyList<ProductoDto>>;

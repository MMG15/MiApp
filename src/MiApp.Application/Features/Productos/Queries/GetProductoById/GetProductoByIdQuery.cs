using MediatR;

namespace MiApp.Application.Features.Productos.Queries.GetProductoById;

public record GetProductoByIdQuery(Guid Id) : IRequest<ProductoDto>;

using MediatR;

namespace MiApp.Application.Features.Productos.Commands.CrearProducto;

public record CrearProductoCommand(
    string Nombre,
    string Descripcion,
    decimal Precio,
    int Stock)
    : IRequest<CrearProductoResponse>;

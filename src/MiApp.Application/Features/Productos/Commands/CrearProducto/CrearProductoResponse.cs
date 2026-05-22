namespace MiApp.Application.Features.Productos.Commands.CrearProducto;

public record CrearProductoResponse(
    Guid Id,
    string Nombre,
    decimal Precio);

namespace MiApp.Application.Features.Productos.Queries.GetProductoById;

public class ProductoDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public int Stock { get; init; }
    public bool IsActive { get; init; }
}

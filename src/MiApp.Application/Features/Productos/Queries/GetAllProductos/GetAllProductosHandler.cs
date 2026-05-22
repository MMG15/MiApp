using MediatR;
using MiApp.Application.Features.Productos.Queries.GetProductoById;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Productos.Queries.GetAllProductos;

public class GetAllProductosHandler : IRequestHandler<GetAllProductosQuery, IReadOnlyList<ProductoDto>>
{
    private readonly IProductRepository _repository;

    public GetAllProductosHandler(IProductRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductoDto>> Handle(GetAllProductosQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        return products.Select(p => new ProductoDto
        {
            Id = p.Id,
            Nombre = p.Name,
            Precio = p.Price,
            Stock = p.Stock,
            IsActive = p.IsActive
        }).ToList().AsReadOnly();
    }
}

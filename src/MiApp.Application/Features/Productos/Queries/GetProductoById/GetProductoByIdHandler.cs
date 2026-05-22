using MediatR;
using MiApp.Application.Common.Exceptions;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Productos.Queries.GetProductoById;

public class GetProductoByIdHandler : IRequestHandler<GetProductoByIdQuery, ProductoDto>
{
    private readonly IProductRepository _repository;

    public GetProductoByIdHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductoDto> Handle(GetProductoByIdQuery request, CancellationToken cancellationToken)
    {
        var producto = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Name,
            Precio = producto.Price,
            Stock = producto.Stock,
            IsActive = producto.IsActive
        };
    }
}

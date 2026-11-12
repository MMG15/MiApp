using MediatR;
using MiApp.Domain.Entities;
using MiApp.Domain.Exceptions;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Productos.Commands.CrearProducto;

public class CrearProductoCommandHandler : IRequestHandler<CrearProductoCommand, CrearProductoResponse>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearProductoCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearProductoResponse> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();

        // El repositorio compara sin distinguir mayúsculas: "Laptop" y "laptop" son el mismo producto
        if (await _repository.ExistsAsync(nombre, cancellationToken))
            throw new DomainException($"Ya existe un producto con el nombre '{nombre}'.");

        var producto = Product.Create(nombre, request.Descripcion, request.Precio, request.Stock);

        await _repository.AddAsync(producto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CrearProductoResponse(producto.Id, producto.Name, producto.Price);
    }
}

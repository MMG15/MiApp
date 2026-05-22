using MediatR;
using MiApp.Application.Common.Exceptions;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Ordenes.Commands.CrearOrden;

public class CrearOrdenCommandHandler : IRequestHandler<CrearOrdenCommand, CrearOrdenResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearOrdenCommandHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearOrdenResponse> Handle(CrearOrdenCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.UserId);

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            product.RemoveStock(item.Quantity);
            order.AddItem(product.Id, item.Quantity, product.Price);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CrearOrdenResponse(order.Id, order.Total, order.Status.ToString(), order.CreatedAt);
    }
}

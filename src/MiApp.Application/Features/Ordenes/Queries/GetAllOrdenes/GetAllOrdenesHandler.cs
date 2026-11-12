using MediatR;
using MiApp.Application.Features.Ordenes.Common;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Ordenes.Queries.GetAllOrdenes;

public class GetAllOrdenesHandler : IRequestHandler<GetAllOrdenesQuery, IReadOnlyList<OrdenDto>>
{
    private readonly IOrderRepository _repository;

    public GetAllOrdenesHandler(IOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<OrdenDto>> Handle(GetAllOrdenesQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync(cancellationToken);
        return orders.Select(OrdenDto.FromEntity).ToList();
    }
}

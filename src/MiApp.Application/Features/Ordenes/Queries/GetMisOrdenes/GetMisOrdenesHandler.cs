using MediatR;
using MiApp.Application.Features.Ordenes.Common;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Ordenes.Queries.GetMisOrdenes;

public class GetMisOrdenesHandler : IRequestHandler<GetMisOrdenesQuery, IReadOnlyList<OrdenDto>>
{
    private readonly IOrderRepository _repository;

    public GetMisOrdenesHandler(IOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<OrdenDto>> Handle(GetMisOrdenesQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        return orders.Select(OrdenDto.FromEntity).ToList();
    }
}

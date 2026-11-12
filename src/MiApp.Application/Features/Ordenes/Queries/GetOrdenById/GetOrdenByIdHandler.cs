using MediatR;
using MiApp.Application.Common.Exceptions;
using MiApp.Application.Features.Ordenes.Common;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Ordenes.Queries.GetOrdenById;

public class GetOrdenByIdHandler : IRequestHandler<GetOrdenByIdQuery, OrdenDto>
{
    private readonly IOrderRepository _repository;

    public GetOrdenByIdHandler(IOrderRepository repository) => _repository = repository;

    public async Task<OrdenDto> Handle(GetOrdenByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (!request.EsAdmin && !order.BelongsTo(request.UserId))
            throw new ForbiddenAccessException("No podés ver una orden que no es tuya.");

        return OrdenDto.FromEntity(order);
    }
}

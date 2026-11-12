using MediatR;
using PaymentService.Application.Common.Exceptions;
using PaymentService.Application.Features.Payments.Common;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Features.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdHandler(IPaymentRepository repository) => _repository = repository;

    public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"No hay un pago registrado para la orden '{request.OrderId}'.");

        return PaymentDto.FromEntity(payment);
    }
}

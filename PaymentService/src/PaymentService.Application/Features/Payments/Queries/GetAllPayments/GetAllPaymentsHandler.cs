using MediatR;
using PaymentService.Application.Features.Payments.Common;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Features.Payments.Queries.GetAllPayments;

public class GetAllPaymentsHandler : IRequestHandler<GetAllPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IPaymentRepository _repository;

    public GetAllPaymentsHandler(IPaymentRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        return payments.Select(PaymentDto.FromEntity).ToList();
    }
}

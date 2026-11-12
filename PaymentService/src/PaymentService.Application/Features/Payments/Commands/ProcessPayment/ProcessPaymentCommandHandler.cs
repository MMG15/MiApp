using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Features.Payments.Common;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // IDEMPOTENCIA: si esta orden ya se procesó, devolvemos el mismo resultado.
        // Importa porque el e-commerce reintenta ante fallas de red: si la primera
        // respuesta se perdió, el reintento no genera un segundo cobro.
        var existing = await _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("La orden {OrderId} ya tenía un pago ({Status}); se devuelve el mismo",
                request.OrderId, existing.Status);
            return PaymentDto.FromEntity(existing);
        }

        // La decisión (aprobar o rechazar) la toma la entidad: el handler solo orquesta
        var payment = Payment.Process(request.OrderId, Money.Create(request.Amount, request.Currency));

        await _repository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pago {TransactionId} de la orden {OrderId}: {Status}",
            payment.TransactionId, payment.OrderId, payment.Status);

        return PaymentDto.FromEntity(payment);
    }
}

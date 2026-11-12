using MediatR;
using MiApp.Application.Common.Exceptions;
using MiApp.Application.Contracts.Infrastructure;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MiApp.Application.Features.Ordenes.Commands.ConfirmarOrden;

/// <summary>
/// Flujo central del trabajo final:
/// 1. Busca la orden y verifica que sea del usuario.
/// 2. Pregunta al dominio si la orden se puede pagar (estado Pending).
/// 3. Llama a PaymentService a través del puerto IPaymentClient (HTTP en Infrastructure).
/// 4. Según la respuesta, cambia el estado de la orden (reglas en la entidad Order).
/// 5. Si el servicio no responde, la orden queda Pending y se informa 503.
/// </summary>
public class ConfirmarOrdenCommandHandler : IRequestHandler<ConfirmarOrdenCommand, ConfirmarOrdenResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentClient _paymentClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmarOrdenCommandHandler> _logger;

    public ConfirmarOrdenCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPaymentClient paymentClient,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmarOrdenCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _paymentClient = paymentClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConfirmarOrdenResponse> Handle(ConfirmarOrdenCommand request, CancellationToken cancellationToken)
    {
        // 1. La orden existe y es del usuario que la confirma
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (!order.BelongsTo(request.UserId))
            throw new ForbiddenAccessException("No podés confirmar una orden que no es tuya.");

        // 2. Regla de dominio: solo órdenes Pending con ítems. Se valida ANTES de llamar
        //    al otro servicio para no cobrar una orden que ya fue pagada o rechazada.
        order.EnsureCanBePaid();

        // 3. Llamada HTTP a PaymentService (a través de la interfaz).
        //    Si falla, el cliente lanza ServiceUnavailableException: no se guarda nada,
        //    la orden sigue Pending y el usuario puede reintentar más tarde.
        var payment = await _paymentClient.ProcessPaymentAsync(
            new PaymentRequest(order.Id, order.Total.Amount, order.Total.Currency),
            cancellationToken);

        // 4. Actuar según la respuesta
        string mensaje;
        if (payment.Approved)
        {
            order.MarkAsPaid(payment.TransactionId);
            mensaje = "Pago aprobado. La orden quedó pagada.";
        }
        else
        {
            order.MarkPaymentRejected(payment.TransactionId);

            // El pago no se hizo: devolvemos al stock lo que la orden tenía reservado
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                product?.AddStock(item.Quantity);
            }

            mensaje = $"Pago rechazado: {(payment.Reason ?? "sin motivo informado").TrimEnd('.')}. Se devolvió el stock.";
        }

        // Estado de la orden + stock devuelto se guardan en una sola transacción
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Orden {OrderId} -> {Status} (transacción {TransactionId})",
            order.Id, order.Status, order.TransactionId);

        return new ConfirmarOrdenResponse(
            order.Id, order.Status.ToString(), order.Total.Amount, order.TransactionId, mensaje);
    }
}

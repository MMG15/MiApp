namespace MiApp.Application.Contracts.Infrastructure;

public interface IPaymentGateway
{
    Task<string> GenerarLinkPagoAsync(Guid pedidoId, decimal monto, CancellationToken ct = default);
}

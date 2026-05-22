namespace MiApp.Application.Contracts.Infrastructure;

public interface IEmailService
{
    Task EnviarConfirmacionPedidoAsync(string email, Guid pedidoId, CancellationToken ct = default);
}

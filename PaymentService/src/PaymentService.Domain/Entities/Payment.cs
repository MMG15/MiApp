using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Un intento de pago de una orden del e-commerce.
///
/// REGLA DE NEGOCIO (vive acá, en el dominio):
///   - Se aprueba si la moneda es ARS y el monto es MENOR a 100.000.
///   - En cualquier otro caso se rechaza, con el motivo.
/// </summary>
public class Payment : BaseEntity
{
    public const decimal ApprovalLimit = 100_000m;
    public const string AcceptedCurrency = "ARS";

    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    // Constructor privado para EF Core
    private Payment() { Amount = null!; TransactionId = null!; }

    // Factory method: la única forma de crear un pago, y ya sale aprobado o rechazado
    public static Payment Process(Guid orderId, Money amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("La orden es obligatoria.");
        if (amount is null)
            throw new DomainException("El monto es obligatorio.");
        if (amount.Amount <= 0)
            throw new DomainException("El monto a pagar debe ser mayor a cero.");

        var (status, reason) = Evaluate(amount);

        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Status = status,
            Reason = reason,
            TransactionId = GenerateTransactionId(),
            ProcessedAt = DateTime.UtcNow
        };
    }

    public bool IsApproved => Status == PaymentStatus.Approved;

    private static (PaymentStatus Status, string? Reason) Evaluate(Money amount)
    {
        if (amount.Currency != AcceptedCurrency)
            return (PaymentStatus.Rejected, $"Solo se aceptan pagos en {AcceptedCurrency}.");

        if (amount.Amount >= ApprovalLimit)
            return (PaymentStatus.Rejected,
                $"El monto {amount} supera el límite de aprobación ({ApprovalLimit} {AcceptedCurrency}).");

        return (PaymentStatus.Approved, null);
    }

    // Ejemplo: TX-9F3C1A2B
    private static string GenerateTransactionId()
        => $"TX-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}

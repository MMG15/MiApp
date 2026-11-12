using MiApp.Domain.Enums;
using MiApp.Domain.Exceptions;
using MiApp.Domain.ValueObjects;

namespace MiApp.Domain.Entities;

public class Order : BaseEntity
{
    // La lista real es privada: desde afuera solo se puede leer.
    // Los ítems se agregan únicamente con AddItem(), que aplica las reglas.
    private readonly List<OrderItem> _items = new();

    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime? PaymentProcessedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Calculado: suma de los subtotales (no se guarda en la base)
    public Money Total => _items.Aggregate(Money.Zero(), (total, item) => total.Add(item.Subtotal));

    private Order() { }

    public static Order Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        return new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Solo se pueden agregar ítems a una orden pendiente.");

        _items.Add(OrderItem.Create(productId, quantity, unitPrice));
    }

    public bool BelongsTo(Guid userId) => UserId == userId;

    // Regla de negocio: solo se paga una orden pendiente y con al menos un ítem
    public void EnsureCanBePaid()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException(
                $"Solo se pueden confirmar órdenes en estado Pending. Estado actual: {Status}.");
        if (_items.Count == 0)
            throw new DomainException("La orden debe tener al menos un ítem.");
    }

    // Transición Pending -> Paid
    public void MarkAsPaid(string transactionId)
    {
        EnsureCanBePaid();
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new DomainException("Un pago aprobado debe tener un número de transacción.");

        Status = OrderStatus.Paid;
        TransactionId = transactionId;
        PaymentProcessedAt = DateTime.UtcNow;
    }

    // Transición Pending -> PaymentRejected
    public void MarkPaymentRejected(string? transactionId)
    {
        EnsureCanBePaid();

        Status = OrderStatus.PaymentRejected;
        TransactionId = transactionId;
        PaymentProcessedAt = DateTime.UtcNow;
    }
}

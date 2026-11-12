using MiApp.Domain.Exceptions;
using MiApp.Domain.ValueObjects;

namespace MiApp.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Calculado: no se guarda en la base (se ignora en la configuración de EF)
    public Money Subtotal => Money.Create(UnitPrice).Multiply(Quantity);

    private OrderItem() { }

    // internal: solo Order (mismo proyecto Domain) puede crear ítems
    internal static OrderItem Create(Guid productId, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("El producto es obligatorio.");
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");
        if (unitPrice <= 0)
            throw new DomainException("El precio unitario debe ser mayor a cero.");

        return new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}

using MiApp.Domain.Enums;
using MiApp.Domain.Exceptions;

namespace MiApp.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);

    private Order() { Items = new List<OrderItem>(); }

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
        var item = OrderItem.Create(productId, quantity, unitPrice);
        Items.Add(item);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Solo se pueden confirmar pedidos en estado Pendiente.");
        if (!Items.Any())
            throw new DomainException("El pedido debe tener al menos un item.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("No se puede cancelar un pedido ya entregado.");

        Status = OrderStatus.Cancelled;
    }
}

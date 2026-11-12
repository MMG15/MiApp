namespace MiApp.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,          // Creada, con stock reservado, esperando el pago
    Paid = 2,             // PaymentService aprobó el pago
    PaymentRejected = 3   // PaymentService rechazó el pago (se devuelve el stock)
}

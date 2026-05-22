namespace MiApp.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int requested, int available)
        : base($"Stock insuficiente para '{productName}'. Solicitado: {requested}, Disponible: {available}") { }
}

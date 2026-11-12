using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.ValueObjects;

/// <summary>Dinero = monto + moneda. Inmutable y con igualdad por valor.</summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    // EF Core usa este constructor para reconstruir el valor desde la base
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "ARS")
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainException("La moneda debe ser un código de 3 letras (por ejemplo ARS).");

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}

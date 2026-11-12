using FluentValidation;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

/// <summary>Valida el FORMATO de la request. La regla de aprobación está en el dominio (Payment).</summary>
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("El orderId es requerido");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a cero");
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es requerida")
            .Length(3).WithMessage("La moneda debe tener 3 letras (por ejemplo ARS)");
    }
}

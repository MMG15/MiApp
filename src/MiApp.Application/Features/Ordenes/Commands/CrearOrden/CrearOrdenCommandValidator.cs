using FluentValidation;

namespace MiApp.Application.Features.Ordenes.Commands.CrearOrden;

public class CrearOrdenCommandValidator : AbstractValidator<CrearOrdenCommand>
{
    public CrearOrdenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("El pedido debe tener al menos un item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("El producto es requerido");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");
        });
    }
}

using FluentValidation;

namespace MiApp.Application.Features.Ordenes.Commands.ConfirmarOrden;

public class ConfirmarOrdenCommandValidator : AbstractValidator<ConfirmarOrdenCommand>
{
    public ConfirmarOrdenCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("La orden es requerida");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es requerido");
    }
}

using FluentValidation;
using MiApp.Domain.Enums;

namespace MiApp.Application.Features.Usuarios.Commands.CambiarRol;

public class CambiarRolCommandValidator : AbstractValidator<CambiarRolCommand>
{
    public CambiarRolCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El usuario es requerido");

        RuleFor(x => x.Rol)
            .NotEmpty().WithMessage("El rol es requerido")
            .IsEnumName(typeof(UserRole), caseSensitive: false)
            .WithMessage("El rol debe ser 'Admin' o 'Customer'");
    }
}

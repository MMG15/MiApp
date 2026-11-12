using MediatR;

namespace MiApp.Application.Features.Usuarios.Commands.CambiarRol;

/// <summary>Un Admin cambia el rol de otro usuario ("Admin" o "Customer").</summary>
public record CambiarRolCommand(Guid UserId, string Rol) : IRequest<CambiarRolResponse>;

public record CambiarRolResponse(Guid Id, string Email, string Rol);

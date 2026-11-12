using MediatR;
using MiApp.Application.Common.Exceptions;
using MiApp.Domain.Entities;
using MiApp.Domain.Enums;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Features.Usuarios.Commands.CambiarRol;

public class CambiarRolCommandHandler : IRequestHandler<CambiarRolCommand, CambiarRolResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarRolCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CambiarRolResponse> Handle(CambiarRolCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var nuevoRol = Enum.Parse<UserRole>(request.Rol, ignoreCase: true);
        user.ChangeRole(nuevoRol);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Ojo: el usuario tiene que volver a loguearse para que su nuevo token tenga el rol nuevo
        return new CambiarRolResponse(user.Id, user.Email.Value, user.Role.ToString());
    }
}

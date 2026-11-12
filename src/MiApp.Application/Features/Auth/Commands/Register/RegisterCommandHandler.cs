using MediatR;
using MiApp.Application.Contracts.Infrastructure;
using MiApp.Domain.Entities;
using MiApp.Domain.Enums;
using MiApp.Domain.Exceptions;
using MiApp.Domain.Interfaces;
using MiApp.Domain.ValueObjects;

namespace MiApp.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Normalizamos ANTES de buscar duplicados: " Ana@X.com" y "ana@x.com" son el mismo email
        var email = Email.Create(request.Email);

        if (await _userRepository.ExistsAsync(email, cancellationToken))
            throw new DomainException($"Ya existe un usuario con el email '{email.Value}'.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        // Todo registro público queda como Customer. Los Admin se crean con el seed
        // o los promueve otro Admin (PUT /api/users/{id}/role).
        var user = User.Create(request.Name, email, passwordHash, UserRole.Customer);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        return new RegisterResponse(user.Id, user.Name, user.Email.Value, user.Role.ToString(), token);
    }
}

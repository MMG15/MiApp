using MediatR;
using MiApp.Application.Contracts.Infrastructure;
using MiApp.Domain.Exceptions;
using MiApp.Domain.Interfaces;
using MiApp.Domain.ValueObjects;

namespace MiApp.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // El Value Object normaliza el email (trim + minúsculas)
        var email = Email.Create(request.Email);

        // Mismo mensaje para "no existe" y "contraseña incorrecta": no damos pistas a un atacante
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new DomainException("Email o contraseña incorrectos.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Email o contraseña incorrectos.");

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(user.Id, user.Name, user.Email.Value, user.Role.ToString(), token);
    }
}

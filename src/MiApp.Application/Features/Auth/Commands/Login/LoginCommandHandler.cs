using MediatR;
using MiApp.Application.Contracts.Infrastructure;
using MiApp.Domain.Exceptions;
using MiApp.Domain.Interfaces;

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
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException("Email o contraseña incorrectos.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Email o contraseña incorrectos.");

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(user.Id, user.Name, user.Email, user.Role.ToString(), token);
    }
}

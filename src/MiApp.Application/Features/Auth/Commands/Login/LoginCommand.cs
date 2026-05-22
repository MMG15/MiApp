using MediatR;

namespace MiApp.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<LoginResponse>;

using MediatR;

namespace MiApp.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Name,
    string Email,
    string Password) : IRequest<RegisterResponse>;
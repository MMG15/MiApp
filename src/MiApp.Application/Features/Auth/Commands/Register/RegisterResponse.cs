namespace MiApp.Application.Features.Auth.Commands.Register;

public record RegisterResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string Token);

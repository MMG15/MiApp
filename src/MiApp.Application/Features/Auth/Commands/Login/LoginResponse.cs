namespace MiApp.Application.Features.Auth.Commands.Login;

public record LoginResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string Token);

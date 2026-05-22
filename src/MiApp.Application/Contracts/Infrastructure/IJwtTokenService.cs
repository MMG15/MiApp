using MiApp.Domain.Entities;

namespace MiApp.Application.Contracts.Infrastructure;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}

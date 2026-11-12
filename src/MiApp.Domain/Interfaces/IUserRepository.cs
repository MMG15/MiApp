using MiApp.Domain.Entities;
using MiApp.Domain.ValueObjects;

namespace MiApp.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsAsync(Email email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

using Microsoft.EntityFrameworkCore;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using MiApp.Domain.ValueObjects;
using MiApp.Infrastructure.Persistence;

namespace MiApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.FindAsync(new object[] { id }, ct);

    // El Email ya viene normalizado por el Value Object; EF usa la conversión para comparar
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsAsync(Email email, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);
}

using MiApp.Domain.Enums;
using MiApp.Domain.Exceptions;

namespace MiApp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }

    private User() { Name = null!; Email = null!; PasswordHash = null!; }

    public static User Create(string name, string email, string passwordHash, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La contraseña es obligatoria.");

        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Role = role
        };
    }
}

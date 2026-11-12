using MiApp.Domain.Enums;
using MiApp.Domain.Exceptions;
using MiApp.Domain.ValueObjects;

namespace MiApp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; }
    public Email Email { get; private set; }   // Value Object: siempre válido y normalizado
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }

    private User() { Name = null!; Email = null!; PasswordHash = null!; }

    public static User Create(string name, Email email, string passwordHash, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre es obligatorio.");
        if (email is null)
            throw new DomainException("El email es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La contraseña es obligatoria.");

        return new User
        {
            Name = name.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void ChangeRole(UserRole newRole)
    {
        if (!Enum.IsDefined(newRole))
            throw new DomainException("El rol indicado no existe.");
        Role = newRole;
    }
}

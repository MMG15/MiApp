using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiApp.Domain.Entities;
using MiApp.Domain.ValueObjects;

namespace MiApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);

        // Value Object Email <-> columna de texto.
        // Al guardar se escribe email.Value; al leer se reconstruye con Email.Create().
        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}

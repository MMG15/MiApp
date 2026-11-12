using MiApp.Application.Contracts.Infrastructure;
using MiApp.Domain.Entities;
using MiApp.Domain.Enums;
using MiApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MiApp.Infrastructure.Persistence.Seed;

/// <summary>
/// Se ejecuta al arrancar la API:
/// 1. Aplica las migraciones pendientes (crea/actualiza miapp.db).
/// 2. Crea el usuario Admin definido en "SeedAdmin" si todavía no existe.
/// Así siempre hay un Admin disponible en runtime para usar los endpoints protegidos.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<AppDbContext>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var passwordHasher = provider.GetRequiredService<IPasswordHasher>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        await context.Database.MigrateAsync();

        var name = configuration["SeedAdmin:Name"];
        var emailText = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(emailText) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No hay sección SeedAdmin configurada: no se crea el Admin inicial.");
            return;
        }

        var email = Email.Create(emailText);
        if (await context.Users.AnyAsync(u => u.Email == email))
            return;

        var admin = User.Create(name ?? "Administrador", email, passwordHasher.Hash(password), UserRole.Admin);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Usuario Admin inicial creado: {Email}", email.Value);
    }
}

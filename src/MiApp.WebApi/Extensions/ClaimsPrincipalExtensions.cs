using System.Security.Claims;

namespace MiApp.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Lee el Id del usuario del claim NameIdentifier del JWT.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("El token no tiene un identificador de usuario válido.");
    }
}

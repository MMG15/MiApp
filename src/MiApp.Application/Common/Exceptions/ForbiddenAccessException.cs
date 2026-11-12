namespace MiApp.Application.Common.Exceptions;

/// <summary>
/// El usuario está autenticado pero no tiene permiso sobre ese recurso
/// (por ejemplo, quiere ver la orden de otro cliente). El middleware la traduce a HTTP 403.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message) { }
}

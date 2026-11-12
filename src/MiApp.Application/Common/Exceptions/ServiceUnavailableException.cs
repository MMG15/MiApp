namespace MiApp.Application.Common.Exceptions;

/// <summary>
/// Un servicio externo (en este sistema, PaymentService) no respondió a tiempo o falló.
/// El middleware la traduce a HTTP 503 Service Unavailable.
/// </summary>
public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

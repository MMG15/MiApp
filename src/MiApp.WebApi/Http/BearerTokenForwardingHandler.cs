using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace MiApp.WebApi.Http;

/// <summary>
/// DelegatingHandler: se mete en el "pipeline" del HttpClient hacia PaymentService
/// y copia el JWT del usuario que hizo la request al e-commerce.
/// Así PaymentService puede validar que la llamada viene de un usuario autenticado
/// (los dos servicios comparten Issuer, Audience y Secret del JWT).
/// </summary>
public class BearerTokenForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BearerTokenForwardingHandler(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();

        if (!string.IsNullOrWhiteSpace(authorization) && AuthenticationHeaderValue.TryParse(authorization, out var header))
            request.Headers.Authorization = header;

        return base.SendAsync(request, cancellationToken);
    }
}

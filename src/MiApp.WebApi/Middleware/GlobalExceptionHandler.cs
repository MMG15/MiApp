using FluentValidation;
using MiApp.Application.Common.Exceptions;
using MiApp.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MiApp.WebApi.Middleware;

/// <summary>
/// Traduce las excepciones de las capas internas a respuestas HTTP con un JSON claro.
/// El orden de los catch importa: primero los tipos más específicos.
/// </summary>
public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)                       // FluentValidation -> 400
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            var errors = ex.Errors.Select(e => new { campo = e.PropertyName, error = e.ErrorMessage });
            await WriteAsync(context, StatusCodes.Status400BadRequest, new { errores = errors });
        }
        catch (ProductNotFoundException ex)                  // hereda de DomainException: va antes -> 404
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status404NotFound, new { error = ex.Message });
        }
        catch (DomainException ex)                           // regla de negocio violada -> 400
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status400BadRequest, new { error = ex.Message });
        }
        catch (NotFoundException ex)                         // -> 404
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status404NotFound, new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)                  // autenticado pero sin permiso -> 403
        {
            _logger.LogWarning("Forbidden: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ServiceUnavailableException ex)               // PaymentService caído o lento -> 503
        {
            _logger.LogWarning("Service unavailable: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
        }
        catch (DbUpdateException ex)                         // p. ej. índice único violado -> 409
        {
            _logger.LogWarning(ex, "Database conflict");
            await WriteAsync(context, StatusCodes.Status409Conflict,
                new { error = "El dato que intentás guardar entra en conflicto con uno existente." });
        }
        catch (Exception ex)                                 // cualquier otra cosa -> 500
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                new { error = "Ha ocurrido un error interno del servidor." });
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(body);
    }
}

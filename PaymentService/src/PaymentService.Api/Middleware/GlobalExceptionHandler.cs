using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Common.Exceptions;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Api.Middleware;

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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            var errors = ex.Errors.Select(e => new { campo = e.PropertyName, error = e.ErrorMessage });
            await WriteAsync(context, StatusCodes.Status400BadRequest, new { errores = errors });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status400BadRequest, new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status404NotFound, new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            // Dos requests simultáneas para la misma orden: el índice único frena la segunda
            _logger.LogWarning(ex, "Database conflict");
            await WriteAsync(context, StatusCodes.Status409Conflict,
                new { error = "Esa orden ya se está procesando o ya fue procesada." });
        }
        catch (Exception ex)
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

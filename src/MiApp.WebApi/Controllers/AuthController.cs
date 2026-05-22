using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Features.Auth.Commands.Login;
using MiApp.Application.Features.Auth.Commands.Register;

namespace MiApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Registrar un nuevo usuario (rol: Customer)</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created(string.Empty, result);
    }

    /// <summary>Iniciar sesión y obtener JWT</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

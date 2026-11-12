using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Features.Usuarios.Commands.CambiarRol;

namespace MiApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]        // -> /api/users
[Authorize(Roles = "Admin")]       // todo este controller es solo para Admin
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Cambiar el rol de un usuario ("Admin" o "Customer"). Solo Admin.
    /// El usuario tiene que volver a loguearse para recibir un token con el rol nuevo.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] CambiarRolRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CambiarRolCommand(id, request.Rol), ct));
}

/// <summary>Body de PUT /api/users/{id}/role.</summary>
public record CambiarRolRequest(string Rol);

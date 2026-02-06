using MediatR;
using Microsoft.AspNetCore.Mvc;
using N5.Permissions.Application.Permissions.Commands.ModifyPermission;
using N5.Permissions.Application.Permissions.Commands.RequestPermission;
using N5.Permissions.Application.Permissions.Queries.GetPermissions;

namespace N5.Permissions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> RequestPermission(
        [FromBody] RequestPermissionCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ModifyPermission(
        int id,
        [FromBody] ModifyPermissionCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return result ? Ok() : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        var query = new GetPermissionsQuery();
        var permissions = await _mediator.Send(query);
        return Ok(permissions);
    }
}

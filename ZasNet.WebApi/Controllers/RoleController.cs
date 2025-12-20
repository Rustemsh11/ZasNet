using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Roles.CreateRole;
using ZasNet.Application.UseCases.Commands.Roles.DeleteRole;
using ZasNet.Application.UseCases.Commands.Roles.UpdateRole;
using ZasNet.Application.UseCases.Queries.Roles.GetRoles;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class RoleController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<List<RoleDto>> GetRoles([FromQuery] GetRolesRequest getRolesRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getRolesRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateRole([FromBody] CreateRoleRequest createRoleRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createRoleRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task UpdateRole([FromBody] UpdateRoleCommand updateRoleCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateRoleCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteRole([FromBody] DeleteRoleCommand deleteRoleCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteRoleCommand, cancellationToken);
    }
}


using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.Users.CreateUser;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task CreateUser([FromBody] CreateUserRequest createUserRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createUserRequest, cancellationToken);
    }
}

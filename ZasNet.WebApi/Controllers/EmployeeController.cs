using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.Users.CreateUser;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task CreateEmployee([FromBody] CreateEmployeeRequest createUserRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createUserRequest, cancellationToken);
    }
}

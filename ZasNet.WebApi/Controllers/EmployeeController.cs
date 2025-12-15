using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Users.CreateUser;
using ZasNet.Application.UseCases.Queries.Employees.GetDispetchers;
using ZasNet.Application.UseCases.Queries.Employees.GetDrivers;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<List<EmployeeDto>> GetDispetchers([FromQuery] GetDispetchersRequest getDispetchersRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getDispetchersRequest, cancellationToken);
    }

    [HttpGet]
    public async Task<List<EmployeeDto>> GetDrivers([FromQuery] GetDriversRequest getDriversRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getDriversRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateEmployee([FromBody] CreateEmployeeRequest createUserRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createUserRequest, cancellationToken);
    }
}

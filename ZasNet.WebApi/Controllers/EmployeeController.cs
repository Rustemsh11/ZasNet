using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Users.CreateUser;
using ZasNet.Application.UseCases.Commands.Users.DeleteEmployee;
using ZasNet.Application.UseCases.Commands.Users.UpdateEmployee;
using ZasNet.Application.UseCases.Queries.Employees.GetDispetchers;
using ZasNet.Application.UseCases.Queries.Employees.GetDrivers;
using ZasNet.Application.UseCases.Queries.Employees.GetEmployees;

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
    public async Task<List<EmployeeDto>> GetEmployees([FromQuery] GetEmployeesRequest getEmployeesRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getEmployeesRequest, cancellationToken);
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
    
    [HttpPost]
    public async Task UpdateEmployee([FromBody] UpdateEmployeeCommand updateEmployeeCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateEmployeeCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteEmployee([FromBody] DeleteEmployeeCommand deleteEmployeeCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteEmployeeCommand, cancellationToken);
    }
}

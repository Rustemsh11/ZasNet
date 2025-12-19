using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.EmployeeEarnings.EmployeeEarningUpdate;
using ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;
using ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class EmployeeEarningController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<GetEmployeeEarningByMounthResponse>> GetEmployeeEarningByMounth([FromQuery] GetEmployeeEarningByMounthRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task UpdateEmployeeEarning([FromBody] EmployeeEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
    }
}

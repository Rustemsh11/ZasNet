using MediatR;
using Microsoft.AspNetCore.Mvc;
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
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.DispetcherEarnings.DispetcherEarningUpdate;
using ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class DispetcherEarningController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<GetDispetcherEarningByMounthResponse>> GetDispetcherEarningByMounth([FromQuery] GetDispetcherEarningByMounthRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task UpdateDispetcherEarning([FromBody] DispetcherEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
    }
}

